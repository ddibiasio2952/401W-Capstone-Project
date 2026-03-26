using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    /// Handles operations related to session data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly AuthService _auth;
        private readonly SessionService _session;
        private readonly AuditService _audit;

        public SessionsController(FalveyInsuranceGroupContext context, AuthService auth, SessionService session, AuditService audit)
        {
            _context = context;
            _auth = auth;
            _session = session;
            _audit = audit;
        }

        /// GET: api/sessions/

        /// <summary>
        /// Gets a list of of all sessions
        /// </summary>
        /// <returns>A list of sessions</returns>
        [HttpGet]
        public async Task<List<SessionDto1>> GetAllSessions()
        {
            var session = await _context.Sessions
                .OrderBy(s => s.created_at)
                .Select(s => new SessionDto1
                {
                    session_id = s.session_id,
                    user_id = s.user_id,
                    session_hash = s.session_hash,
                    created_at = s.created_at,
                    expires_at = s.expires_at,
                    revoked_at = s.revoked_at,
                    ip_address = s.ip_address,
                    user_agent = s.user_agent,
                    is_active = s.is_active
                }).
            ToListAsync();
            return session;
        }

        /// <summary>
        /// Gets all of a user's active sessions
        /// </summary>
        /// <returns>An ActionResult that contains a list of the user's active sessions</returns>
        /// <response code="200">Method successfully terminated</response>
        /// <response code="500">Possible server issues</response>
        [HttpGet("my-sessions")]
        public async Task<ActionResult<Object>> GetUserSessions()
        {
            var user_id = HttpContext.Session.GetInt32("UserID");
            var session_hash = HttpContext.Session.GetString("SessionID");

            // checks to see if cookies or session variables have not been cleared
            if (user_id == null || session_hash == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            

            Object active_sessions; // The Object that contains the current active session and the other active sessions
            try
            {
                active_sessions = await _session.GetActiveSessions(user_id, session_hash);
            }
            catch (KeyNotFoundException) // error is thrown when no Session matches the session_hash stored in session state
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            
            return Ok(active_sessions);

        }    


        /// <summary>
        /// Fetches a session by its hash and revokes it
        /// </summary>
        /// <param name="session_hash">The hash used to retrieve a Session</param>
        /// <returns>A task that contains an ActionResult object</returns>
        /// <response code="200">Method successfully terminated</response>
        [HttpPost("revoke")]
        public async Task<ActionResult> RevokeSession([FromBody] string session_hash)
        {
            session_hash = session_hash.Trim('"');
            var session = await _session.GetSession(session_hash);
            
            if (session == null)
            {
                return Ok();
            }
            
            // checks if a session has not been revoked 
            // for edge cases where a user already revoked in a different browser
            if (session.revoked_at == null)
            {
                await _session.RevokeSession(session);
                await _audit.RevokeSuccess(session); // create audit indicating session was forcefully closed
            }

            return Ok();

        }
    
        /// <summary>
        /// Checks to see if a session's state in the db is valid and performs necessary updates/actions
        /// </summary>
        /// <returns></returns>
        [HttpPost("validate")]
        public async Task<ActionResult> SessionValidator()
        {
            bool cookie_exist = _auth.DoesCookieExist(HttpContext);
            var session_hash = HttpContext.Session.GetString("SessionID");

            // checks to see if cookie or session hash has been cleared
            if (!cookie_exist || session_hash == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            // gets session via session hash and checks to see if it actually exists
            var session = await _session.GetSession(session_hash);
            if (session == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

           // checks to see if a session has expired, closed, or been revoked
           bool is_valid = await _session.ValidateSession(session);
           if (!is_valid)
            {
                HttpContext.Session.Clear(); // clears session variables in session store
                Response.Cookies.Delete(".AspNetCore.Session"); // instructs browser to delete session cookie
                return Unauthorized();
            }
           
           return Ok();
            
        }

        
        [HttpGet("get-id")]
        public ActionResult GetUserId()
        {
            int? user_id = HttpContext.Session.GetInt32("UserID");

            if (user_id == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(user_id);
        }

        [HttpGet("get-timer")]
        public ActionResult GetTimerVal()
        {
            string? timer_val = HttpContext.Session.GetString("TimerVal");

            if (timer_val.IsNullOrEmpty())
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(timer_val);
        }


        [HttpPost("set-timer")]
        public ActionResult SetTimerVal([FromBody] string timer_as_ms)
        {
            string? timer_val = HttpContext.Session.GetString("TimerVal");

            if (timer_val.IsNullOrEmpty())
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            timer_val = timer_as_ms.Trim('"');
            HttpContext.Session.SetString("TimerVal", timer_val);

            return Ok();
        }
        

        /// <summary>
        /// Creates a session hash
        /// </summary>
        /// <returns> A unique session hash </returns>
        public static string newSessionHash()
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + DateTime.Now);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }

    }
}
