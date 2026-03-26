using FalveyInsuranceGroup.Db;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Backend.Models;

namespace FalveyInsuranceGroup.Backend.Services
{
    /// <summary>
    /// The business logic for authentication operations like validation for login
    /// </summary>
    public class AuthService
    {
        private readonly FalveyInsuranceGroupContext _context;

        public AuthService(FalveyInsuranceGroupContext context)
        {
            _context = context;

        }

        /// <summary>
        /// Checks to see if the a user with the provided email exists
        /// Checks if the provided password matches the hash stored with the fetched user
        /// </summary>
        /// <param name="user_email">The email to validate</param>
        /// <param name="password">The password to hash and validate</param>
        /// <returns>A bool value</returns>
         public async Task<(User? user, bool success)> validateUser(string user_email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.email == user_email);

            // Checks if a user was fetched or if the given password's hash matches the one in db
            if (user == null || !BCrypt.Net.BCrypt.EnhancedVerify(password, user.password_hash))
            {
                return (user, false);
            }

            return (user, true);


        }

        /// <summary>
        /// Checks to see if the user's browser holds a session cookie
        /// </summary>
        /// <param name="http">Session ID is extracted from the session cookie</param>
        /// <returns>A bool value indicating if a session cookie exists</returns>
        public bool DoesCookieExist(HttpContext http)
        {
           var session_cookie = http.Request.Cookies[".AspNetCore.Session"];

           if (session_cookie == null)
            {
                return false;
            }

            return true;
        }

    }





}