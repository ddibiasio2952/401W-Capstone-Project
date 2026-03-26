using FalveyInsuranceGroup.Db;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Dtos;

namespace FalveyInsuranceGroup.Backend.Services
{
    /// <summary>
    /// handles the business logic for logging and auditing operations
    /// </summary>
    public class AuditService
    {
        private readonly FalveyInsuranceGroupContext _context;

        public AuditService(FalveyInsuranceGroupContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Fetches all of user's login audits from db
        /// </summary>
        /// <param name="user_id">The ID used to fetch the login audits</param>
        /// <returns>A list of a user's login history in the form of LoginAuditDtos</returns>
        public async Task<List<LoginAuditDto>> GetUserLoginAudits(int? user_id)
        {
            var user_audits = await _context.LoginAudits
                .Where(l => l.user_id == user_id)
                .Select(l => new LoginAuditDto
                {
                    login_event = l.login_event,
                    ip_address = l.ip_address,
                    user_agent = l.user_agent,
                    occurred_at = l.occurred_at
                }).
                ToListAsync();

            return user_audits;
        }


        /// <summary>
        /// Creates a LoginAudit that indicates successful login and adds it to the database
        /// </summary>
        /// <param name="new_user_id">Given ID that states which user logged in</param>
        /// <param name="http_context">Info from the context is used</param> 
        /// <returns>A Task object</returns>
        public async Task LoginSuccess(int new_user_id, HttpContext http_context)
        {
            // creates an audit that uses info from a Http request like a ip address
            var new_audit = new LoginAudit
            {
                user_id = new_user_id,
                login_event = "sign in success",
                ip_address = http_context.Connection.RemoteIpAddress?.ToString(),
                user_agent = http_context.Request.Headers["User-Agent"].ToString(),
            };

            // adds the audit into the db
            _context.LoginAudits.Add(new_audit);
            await _context.SaveChangesAsync();
        }


        /// <summary>
        /// Creates a LoginAudit that indicates login failure and adds it to db
        /// </summary>
        /// <param name="new_user_id">Given ID that states which user logged in</param>
        /// <param name="http">Info from the context is used</param>
        /// <returns>A Task object</returns>
        public async Task LoginFail(int new_user_id, HttpContext http)
        {
            var new_audit = new LoginAudit
            {
                user_id = new_user_id,
                login_event = "sign in failure",
                ip_address = http.Connection.RemoteIpAddress?.ToString(),
                user_agent = http.Request.Headers["User-Agent"].ToString()
            };

            _context.LoginAudits.Add(new_audit);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a LoginAudit that indicates sign out and adds it to db
        /// </summary>
        /// <param name="new_user_id">Given ID that states which user logged in</param>
        /// <param name="http">Info from the context is used</param>
        /// <returns>A Task object</returns>
        public async Task SignOutSuccess(int new_user_id, HttpContext http)
        {
            var new_audit = new LoginAudit
            {
                user_id = new_user_id,
                login_event = "sign out",
                ip_address = http.Connection.RemoteIpAddress?.ToString(),
                user_agent = http.Request.Headers["User-Agent"].ToString()
            };

            _context.LoginAudits.Add(new_audit);
            await _context.SaveChangesAsync();
        }



        /// <summary>
        /// Creates a LoginAudit that indicates a session was forcefully signed out
        /// and adds it to db
        /// </summary>
        /// <param name="session">The session that was revoked</param>
        /// <returns>A Task Object</returns>
         public async Task RevokeSuccess(Session session)
        {
            var new_audit = new LoginAudit
            {
                user_id = session.user_id,
                login_event = "signed out single session",
                ip_address = session.ip_address,
                user_agent = session.user_agent,
            };

            _context.LoginAudits.Add(new_audit);
            await _context.SaveChangesAsync();
        }
    }


}