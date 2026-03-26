using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Db;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Dtos;
using System.Linq.Expressions;
using FalveyInsuranceGroup.Backend.Services;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    /// A controller that uses http methods to modify and fetch from a database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoginAuditController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context; // contains the database context
        private readonly InputService _service;
        private readonly string[] ALLOWED_EVENT = {"sign in success", "sign out failure",
                                                   "sign out", "signed out single session"};

        // constructor that sets private variable with database context
        public LoginAuditController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// <summary>
        /// Gets a list of login audits
        /// </summary>
        /// <returns>A list of login audits</returns>
        [HttpGet]
        public async Task<List<LoginAuditDto1>> GetLoginAudits()
        {
            var listLoginAudit = await _context.LoginAudits
            .AsNoTracking()
            .Select(mapToLoginAuditDto)
            .ToListAsync();

            return listLoginAudit;
        }

        /// <summary>
        /// Gets a login audit by id
        /// </summary>
        /// <param name="id">The login audit to fetch</param>
        /// <returns>The specified login audit</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<LoginAuditDto1>> GetAuditById(long id)
        {
            var logAudit = await _context.LoginAudits.FindAsync(id);

            if (logAudit == null)
            {
                return NotFound($"Login audit with ID {id} was not found");
            }

            return createLoginAuditDto(logAudit);
        }


        /// <summary>
        /// Adds a LoginAudit entity into the database
        /// </summary>
        /// <param name="dto">Holds the data that will be added into the database</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<LoginAudit>> PostLoginAudit([FromBody] LoginAuditDto1 dto)
        {
            if (dto.audit_id.HasValue)
            {
                return BadRequest("Audit ID cannot be provided on creation");
            }


            if (!await hasValidUserId(dto.user_id)) {
            
                return BadRequest("User ID is not valid");
            }


            // checks if the LoginAudit has a valid event entry
            if (!_service.hasValidEnumType(ALLOWED_EVENT, dto.login_event))
            {
                return BadRequest("Not a valid event entry.");
            }

            var auditToPost = new LoginAudit
            {
                user_id = dto.user_id,
                login_event = dto.login_event,
                ip_address = dto.ip_address,
                user_agent = dto.user_agent,
                occurred_at = dto.occurred_at
            };

            _context.LoginAudits.Add(auditToPost); // adds it to the DbSet
            await _context.SaveChangesAsync(); // uploads new data in DbSet to database

            return CreatedAtAction(nameof(GetAuditById), new { id = auditToPost.audit_id }, auditToPost);


        }

        /// <summary>
        /// Fetches an existing LoginAudit and updates it with the dto's values
        /// </summary>
        /// <param name="id">To fetch a LoginAudit by ID</param>
        /// <param name="dto">The dto's values are used to update the fetched entity</param>
        /// <returns>No content</returns>
        /// <response code="404">Not found</response>
        /// <response code="400">Bad request</respose>
        [HttpPut("{id}")]
        public async Task<ActionResult<LoginAudit>> PutLoginAudit(long id, [FromBody] LoginAuditDto1 dto)
        {

            if (dto.audit_id.HasValue) {
                return BadRequest("Audit ID cannot be provided on creation");
            }


            if (!await hasValidUserId(dto.user_id)) {
            
                return BadRequest("User ID is not valid");
            }

             // checks if the LoginAudit has a valid event entry
            if (!_service.hasValidEnumType(ALLOWED_EVENT, dto.login_event))
            {
                return BadRequest("Not a valid event entry.");
            }


            // fetches the entity to be updated
            var auditToUpdate = await _context.LoginAudits.FindAsync(id);

            // checks to see if the fetched entity exists
            if (auditToUpdate == null)
            {
                return NotFound($"LoginAudit with the ID {id} does not exist.");
            }

            // overwrites values in fetched entity
            auditToUpdate.user_id = dto.user_id;
            auditToUpdate.login_event = dto.login_event;
            auditToUpdate.ip_address = dto.ip_address;
            auditToUpdate.user_agent = dto.user_agent;
            auditToUpdate.occurred_at = dto.occurred_at;

            // save changes to database
            await _context.SaveChangesAsync();
            return NoContent();

        }

        /// <summary>
        /// Fetches a LoginAudit to delete it from the database
        /// </summary>
        /// <param name="id">The ID to fetch the LoginAudit entity</param>
        /// <returns>No content</returns>
        /// <response code="404">Resource could not be found</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult<LoginAudit>> DeleteAuditById(long id)
        {
            var auditToDelete = await _context.LoginAudits.FindAsync(id); // fetches entity to be deleted

            // checks to see if entity exists
            if (auditToDelete == null)
            {
                return NotFound();
            }

            _context.LoginAudits.Remove(auditToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Creates a LoginAuditDto entity with a LoginAudit's values
        /// </summary>
        /// <param name="login_audit">The values found in this entity are used</param>
        /// <returns>A LoginAuditDto entity to add in a database</returns>
        private LoginAuditDto1 createLoginAuditDto(LoginAudit login_audit)
        {
            return new LoginAuditDto1
            {
                audit_id = login_audit.audit_id,
                user_id = login_audit.user_id,
                login_event = login_audit.login_event,
                ip_address = login_audit.ip_address,
                user_agent = login_audit.user_agent,
                occurred_at = login_audit.occurred_at

            };

        }

        /// <summary>
        /// Takes a LoginAudit and maps it to a LoginAuditDto
        /// Then it represents it as data so it can be converted to a SQL query
        /// </summary>
        private Expression<Func<LoginAudit, LoginAuditDto1>> mapToLoginAuditDto = l => new LoginAuditDto1
        {
            audit_id = l.audit_id,
            user_id = l.user_id,
            login_event = l.login_event,
            ip_address = l.ip_address,
            user_agent = l.user_agent,
            occurred_at = l.occurred_at
        };

        /// <summary>
        /// Checks to see if a LoginAuditDto entity contains the same ID as the given ID parameter
        /// </summary>
        /// <param name="audit_user_id"></param>
        /// <returns>A task with bool result</returns>
        private async Task<bool> hasValidUserId(int audit_user_id)
        {
            return await _context.Users.AnyAsync(u => u.user_id == audit_user_id);

        }
    }

}
