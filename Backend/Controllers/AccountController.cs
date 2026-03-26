using Microsoft.AspNetCore.Mvc;
using FalveyInsuranceGroup.Backend.Services;
using Microsoft.IdentityModel.Tokens;

namespace FalveyInsuranceGroup.Backend.Controllers
{

    /// <summary>
    /// handles registration, updating user profile, and fetching user profile with their specific hub
    /// e.g. signing in as an admin would fetch their hub with a feature that no other user has
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly AccountService _acc_service;

        public AccountController(AccountService acc_service)
        {
            _acc_service = acc_service;
        }

        /// <summary>
        /// creates a new user with a given email and password
        /// </summary>
        /// <param name="email">The email to sign up with</param>
        /// <param name="password">The password to be hashed and stored</param>
        /// <param name="id"> The employee ID tied to a user</param>
        /// <returns>An ActionResult object</returns>
        /// <response code="200">Returns message indicating success</response>
        /// <response code="400"> Given parameters could not register a new user</response>
        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser(string email, string password, int id)
        {
            bool is_created = await _acc_service.CreateUser(email, password, id);

            // checks for case where user could not be created
            if (!is_created)
            {
                return BadRequest("Email or ID is unavailable");
            }

            return Ok("Account successfully created");
        }




        [HttpGet("account-name")]
        public ActionResult GetAccountName()
        {
            string? account_name = HttpContext.Session.GetString("AccountName");

            if (account_name.IsNullOrEmpty())
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(account_name);
        }
    }

}
