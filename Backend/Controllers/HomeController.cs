using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FalveyProject.Backend.Controllers
{
    /// <summary>
    /// Redirect user to different pages or provides HTML pages
    /// </summary>
    [Route("Home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public HomeController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Provides the Dashboard HTML page using its file path
        /// </summary>
        /// <returns>Dashboard HTML page</returns>
        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            string file_path = Path.Combine(_env.WebRootPath, "Dashboard", "dashboard.html");

            
            return PhysicalFile(file_path, "text/html");
        }


        /// <summary>
        /// Provides the Login HTML page using its file path
        /// </summary>
        /// <returns>Login HTML page</returns>
        [HttpGet("login-page")]
        public IActionResult GetLoginPage()
        {
            string file_path = Path.Combine(_env.WebRootPath, "Login", "login.html");

            return PhysicalFile(file_path, "text/html");
        }


        /// <summary>
        /// Redirect user to login page if session does not exist
        /// If session exist, redirect to dashboard
        /// </summary>
        /// <returns>URL of a controller</returns>
        [HttpGet("/")]
        public IActionResult GoToHomePage()
        {
            string? session_hash = HttpContext.Session.GetString("SessionID");

            // If session does not exist, force user to login to access resources
            if (session_hash.IsNullOrEmpty())
            {
                return Redirect("/home/login-page");

            }
            else 
            {
                return Redirect("/home/dashboard");
            }

        }


        /// <summary>
        /// If user accesses login page through history, this checks if a session is already open on the browser
        /// </summary>
        /// <returns>An Ok result or a Redirect result</returns>
        [HttpGet("session")]
        public ActionResult checkSessionConnection()
        {
            string? id = HttpContext.Session.GetString("SessionID");

            if (!id.IsNullOrEmpty())
            {
                return Unauthorized("https://localhost:7288/home/dashboard"); // a session is already open so need to login again
            }

           return Ok();
        }

    }
}