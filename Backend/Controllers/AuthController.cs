using Microsoft.AspNetCore.Mvc;
using FalveyInsuranceGroup.Backend.Services;
using Microsoft.IdentityModel.Tokens;

namespace FalveyInsuranceGroup.Backend.Controllers
{

    /// <summary>
    /// handles login, token management, and more
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth_service;
        private readonly SessionService _session;
        private readonly AuditService _audit;
        private readonly HelperService _helper;

        public AuthController(AuthService auth_service, SessionService session, AuditService audit, HelperService helper)
        {
            _auth_service = auth_service;
            _session = session;
            _audit = audit;
            _helper = helper;
        }

        /// <summary>
        /// validates the provided email and password, and logs them in with an active session
        /// </summary>
        /// <param name="email">The email string to be validated</param>
        /// <param name="password">The password string to be validated</param>
        /// <returns>An HTTP response that specifies whether login was successful or not </returns>
        /// <response code="200">Returns message indicating success</response>
        /// <response code="401">Could not login user due to incorrect input</response>
        [HttpPost("login")]
        public async Task<ActionResult> LoginUser(string email, string password)
        {
            // returns a tuple that should contain a user (or null) and a bool value
            var result = await _auth_service.validateUser(email, password);

            // checks if validation process was a fail
            if (!result.success)
            {   
                // checks to see if a user was returned and creates an audit for that user
                if (result.user != null)
                {
                    await _audit.LoginFail(result.user.user_id, HttpContext); // creates audit that indicates failed sign-in
                }
                
                return Unauthorized("Invalid email or password.");
            }

            var employee = await _helper.GetEmployee(result.user.employee_id);

            if (employee == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }


            await _session.CreateSession(result.user.user_id, employee.name, HttpContext);
            await _audit.LoginSuccess(result.user.user_id, HttpContext); // creates audit that indicates successful login


            return Ok("Login successful");

        }


        /// <summary>
        /// Logs a user off and performs necessary actions with the session
        /// </summary>
        /// <returns>An OkResult object</returns>
        /// <response code="200">The user is able to log off</response>
        [HttpPost("logoff")]
        public async Task<ActionResult> LogOffUser()
        {
            var cookie_exists = _auth_service.DoesCookieExist(HttpContext);
            if (!cookie_exists)
            {
                return Ok(); // no further action can be done if session cookie does not exist
            }


            // checks to see if a session hash exists in the session store
            var session_hash = HttpContext.Session.GetString("SessionID");
            if (session_hash != null)
            {
                // fetches a Session via session hash and checks to see if that session actually exists
                var session = await _session.GetSession(session_hash);
                if (session != null)
                {
                    await _session.CloseSession(session);
                    await _audit.SignOutSuccess(session.user_id, HttpContext); // creates sign-off audit
                }
                
            }

            HttpContext.Session.Clear(); // clears session variables in the session store
            Response.Cookies.Delete(".AspNetCore.Session"); // instructs browser to drop session cookie
            return Ok();
        

        }

        


        [HttpGet("session")]
        public ActionResult GetSessionHash()
        {
            string? id = HttpContext.Session.GetString("SessionID");

            if (id.IsNullOrEmpty())
            {
                return Unauthorized("No session found");
            }

            return Ok(id);
    
        }

    }



}

