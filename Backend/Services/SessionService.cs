using FalveyInsuranceGroup.Db;
using FalveyInsuranceGroup.Backend.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Backend.Dtos;


namespace FalveyInsuranceGroup.Backend.Services
{
    /// <summary>
    /// Handles business logic of sessions
    /// </summary>
    public class SessionService
    {
        private readonly FalveyInsuranceGroupContext _context;

        public SessionService(FalveyInsuranceGroupContext context)
        {
            _context = context;
        }


       /// <summary>
       /// Creates a new Session object and adds it to the db
       /// Creates session variables to be stored in the session store
       /// </summary>
       /// <param name="new_user_id">Used to tie a user to a session</param>
       /// <param name="http">Info from context is used to create the session</param>
       /// <returns>A Task object</returns>
        public async Task CreateSession(int new_user_id, string employee_name, HttpContext http)
        {
            
            var hash = NewSessionHash();
            var new_session = new Session
            {
                session_id = Guid.NewGuid(),
                user_id = new_user_id,
                session_hash = hash,
                created_at = DateTime.UtcNow,
                expires_at = DateTime.UtcNow.AddMinutes(10),
                revoked_at = null,
                ip_address = http.Connection.RemoteIpAddress?.ToString(),
                user_agent = http.Request.Headers["User-Agent"].ToString()
                
            };
            

            http.Session.SetString("SessionID", hash);
            http.Session.SetInt32("UserID", new_user_id);
            http.Session.SetString("AccountName", employee_name);

            var ms_to_min = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 2 * 60 * 1000;
            http.Session.SetString("TimerVal", ms_to_min.ToString());
            
            _context.Sessions.Add(new_session);
            await _context.SaveChangesAsync();
            
        }

        
        /// <summary>
        /// It set a session to closed
        /// </summary>
        /// <param name="session">The Session to be closed</param>
        /// <returns>Returns a Task that represent an asynchronous operation</returns>
        public async Task CloseSession(Session session)
        {

            session.is_active = false;
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// It revokes a specific Session and updates the database
        /// </summary>
        /// <param name="session">The session to be revoked</param>
        /// <returns>Returns a Task that represents an asynchronous operation</returns>
        public async Task RevokeSession(Session session)
        {
            session.revoked_at = DateTime.UtcNow;
            session.is_active = false;
            await _context.SaveChangesAsync();

        }


        /// <summary>
        /// Checks if a session is valid (like expiration) and takes the necessary
        /// steps to update db if session is invalid
        /// </summary>
        /// <param name="session">The session to be validated</param>
        /// <returns></returns>
        public async Task<bool> ValidateSession(Session session)
        {
            // this check accounts for when a session is revoked
            if (session.is_active == false)
            {
                return false;
            }
            else if (session.expires_at <= DateTime.UtcNow)
            {
                await CloseSession(session);
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// Fetches a Session via a session hash
        /// </summary>
        /// <param name="session_hash">Used to fetch a Session object</param>
        /// <returns>A Session object</returns>
        public async Task<Session?> GetSession(string session_hash)
        {
            return await _context.Sessions.FirstOrDefaultAsync(s => s.session_hash == session_hash);
        }


        /// <summary>
        /// Retrieves all the active sessions of a user including the current one in use
        /// </summary>
        /// <param name="user_id">The ID needed to retrieve the relevant Sessions</param>
        /// <param name="current_hash">The hash that finds the current session</param>
        /// <returns>An object that contains the current session and a list of all the other sessions</returns>
        /// <exception cref="KeyNotFoundException">If no active session matches the hash of the current session</exception>
        public async Task<Object> GetActiveSessions(int? user_id, string current_hash)
        {
            
            /* It finds all the sessions that are tied to the user and are active.
             * It transforms each one into a SessionDto object.
             * Finally, it puts those dto objects into a List object
            */
            var other_sessions = await _context.Sessions
                .Where(s => s.user_id == user_id && s.is_active == true)
                .Select(s => new SessionDto
                {
                    session_hash = s.session_hash,
                    created_at = s.created_at,
                    ip_address = s.ip_address,
                    user_agent = s.user_agent,
                }).
                ToListAsync();

            // This condition finds which SessionDto is the current session
            Predicate<SessionDto> is_current = s => s.session_hash == current_hash;

            var current_session = other_sessions.Find(is_current); // filters the list to find the current session
            if (current_session == null)
            {
                throw new KeyNotFoundException();
            }

            other_sessions.Remove(current_session); 

            return new {current_session, other_sessions};
        }



        /// <summary>
        /// Creates a unique session hash
        /// </summary>
        /// <returns>A string value of a session hash</returns>
        private string NewSessionHash()
        {
            var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString() + DateTime.Now);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }

    }

}