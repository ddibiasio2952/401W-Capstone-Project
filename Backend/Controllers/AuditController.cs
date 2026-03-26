using Microsoft.AspNetCore.Mvc;
using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Db;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    /// Handles login audit operations, session logging, etc.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {

        private readonly FalveyInsuranceGroupContext _context;
        private readonly AuditService _audit;

        public AuditController(FalveyInsuranceGroupContext context, AuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        
        /// <summary>
        /// Fetches all of a user's login history
        /// </summary>
        /// <returns>A list of a user's login audits</returns>
        /// <response code="200">Method successfully terminated </response>
        /// <response code="500">Could not find user stored in session store b/c of possible server issues</response>
        [HttpGet("audits")]
        public async Task<ActionResult<List<LoginAuditDto>>> GetLoginAudits()
        {
            // fetches the user id stored in session store
            var user_id = HttpContext.Session.GetInt32("UserID");

            if (user_id == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }


            // gets all the login audits tied to a user
            var user_audits = await _audit.GetUserLoginAudits(user_id);

            return Ok(user_audits);

        }



    }




}