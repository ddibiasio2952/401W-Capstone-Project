using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Db;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Backend.Dtos;
using System.Linq.Expressions;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    /// Handles operations related to claim data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;
        private static readonly  string[] ALLOWED_STATUS = { "Open", "Investigating", "Pending", "Approved", "Denied", "Closed" };
        
        public ClaimsController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// GET: api/claims

        /// <summary>
        /// Gets a list of claims
        /// </summary>
        /// <returns>A list of claims</returns>
        [HttpGet]
        public async Task<List<ClaimDto>> getClaims()
        {
            var claims = await _context.Claims
            .AsNoTracking()
            .Include(c => c.claim_policy)
            .Include(c => c.assigned_employee)
            .Include(c => c.user_uploader)
            .Select(MapToClaimDto)
            .ToListAsync();

            return claims;
        }

        /// GET: api/claims/id

        /// <summary>
        /// Gets a claim by ID
        /// </summary>
        /// <param name="id"> The id of the claim to retrieve</param>
        /// <returns>A claim dto based on provided ID</returns>
        /// <response code="200">Returns the claim</response>
        /// <response code="404">If the claim is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClaimDto>> getClaim(int id)
        {
            var claim = await _context.Claims
            .AsNoTracking()
            .Include(c => c.claim_policy)
            .Include(c => c.assigned_employee)
            .Include(c => c.user_uploader)
            .Select(MapToClaimDto)
            .FirstOrDefaultAsync(c => c.claim_id == id);

            if (claim == null)
            {
                return NotFound($"Claim with ID {id} not found");
            }

            return Ok(claim);
        }

        /// GET: api/claims/search?policy_id

        /// <summary>
        /// Gets a list of claims based on a Policy ID query
        /// </summary>
        /// <returns>A list of claims based on a Policy ID query</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ClaimDto>>> getClaimsByPolicy([FromQuery] int? policy_id)
        {
            var claim = _context.Claims
                .Select(c => new ClaimDto
                {
                    claim_id = c.claim_id,
                    policy_id = c.policy_id,
                    claim_number = c.claim_number,
                    status = c.status,
                    date_of_loss = c.date_of_loss,
                    date_reported = c.date_reported,
                    reserve_amount = c.reserve_amount,
                    paid_amount = c.paid_amount,
                    memo = c.memo,
                    assigned_to = c.assigned_to,
                    assigned_employee = c.assigned_employee != null ? c.assigned_employee.name : "Unknown",
                    created_by = c.created_by,
                    created_at = c.created_at
                });


            if (policy_id.HasValue)
            {
                claim = claim.Where(c => c.policy_id == policy_id.Value);
            }

            var results = await claim.ToListAsync();
            return Ok(results);
        }

        /// PUT: api/claims/id

        /// <summary>
        ///  Updates an existing claim by ID
        /// </summary>
        /// <param name="id">The claim to update</param>
        /// <param name="updated_claim">Claim object that holds the new details</param>
        /// <returns>No content</returns>
        /// <response code="204">The update was a success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Record not found</response>
        [HttpPut("{id}")]
        public async Task<ActionResult> updateClaim(int id, [FromBody] ClaimDto dto)
        {
            var claim = await _context.Claims.FindAsync(id);
            var current = DateTime.Now;

            if (!_service.hasValidEnumType(ALLOWED_STATUS, dto.status)) {
                return BadRequest("Invalid status input");
            }

            // Ensures an existing policy is used
            if (!await _service.hasValidPolicy(dto.policy_id)) {
                return BadRequest("The given policy ID does not exist.");
            }
            // Check if claim is null.
            if (claim == null) {
                return NotFound($"Claim with ID {id} not found");
            }

            // Check for future dates.
            if (dto.date_of_loss > current)
            {
                return BadRequest("Date of loss is after current date.");
            }
            if (dto.date_reported > current)
            {
                return BadRequest("Date reported is after current date.");
            }


            // Update fields
            claim.policy_id = dto.policy_id;
            claim.claim_number = dto.claim_number;
            claim.status = dto.status;
            claim.date_of_loss = dto.date_of_loss;
            claim.date_reported = dto.date_reported;
            claim.reserve_amount = dto.reserve_amount;
            claim.paid_amount = dto.paid_amount;
            claim.memo = dto.memo;
            claim.assigned_to = dto.assigned_to;
            claim.created_by = dto.created_by;
            await _context.SaveChangesAsync();

            return Ok(claim);
        }


        /// POST: api/claims

        /// <summary>
        /// Adds a new claim
        /// </summary>
        /// <param name="dto">Claim object to add</param>
        /// <returns>The new record</returns>
        /// <response code="200">Record added successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        public async Task<ActionResult> addClaim([FromBody] ClaimDto dto)
        {
            var current = DateTime.Now;
            // Ensures if a given claim number is unique
            if (await _service.hasDuplicateClaimNumber(dto.claim_number)) {
                return BadRequest("The given claim number is already in use");
            }
             
            // Ensures an existing policy is used
            if (!await _service.hasValidPolicy(dto.policy_id)) {
                return BadRequest("The given policy ID does not exist");
            }

            if (!_service.hasValidEnumType(ALLOWED_STATUS, dto.status)) {
                return BadRequest("Invalid status input");
            }

            // Check for future dates.
            if (dto.date_of_loss > current)
            {
                return BadRequest("Date of loss is after current date.");
            }
            if (dto.date_reported  > current)
            {
                return BadRequest("Date reported is after current date.");
            }
            
            var new_claim = new Claim
            {
                policy_id = dto.policy_id,
                claim_number = dto.claim_number,
                status = dto.status,
                date_of_loss = dto.date_of_loss,
                date_reported = dto.date_reported,
                reserve_amount = dto.reserve_amount,
                paid_amount = dto.paid_amount,
                memo = dto.memo,
                assigned_to = dto.assigned_to,
                created_by = dto.created_by,
            };

            _context.Claims.Add(new_claim);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(getClaim), new { id = new_claim.claim_id }, createClaimDto(new_claim));
        }


        /// DELETE: api/claims/id

        /// <summary>
        /// Deletes an existing claim
        /// </summary>
        /// <param name="id">The claim to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">Deletion was a success</response>
        /// <response code="404">Claim not found</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult> deleteClaim(int id)
        {
            var claim = await _context.Claims.FindAsync(id);

            if (claim == null)
            {
                return NotFound($"Claim with ID {id} not found");
            }

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Creates a dto using a claim entity model
        /// </summary>
        /// <param name="c">The claim entity model</param>
        /// <returns>A claim dto with the only necessary information</returns>
        private ClaimDto createClaimDto(Claim c)
        {
            return new ClaimDto
            {
                claim_id = c.claim_id,
                policy_id = c.policy_id,
                claim_number = c.claim_number,
                status = c.status,
                date_of_loss = c.date_of_loss,
                date_reported = c.date_reported,
                reserve_amount = c.reserve_amount,
                paid_amount = c.paid_amount,
                memo = c.memo,
                assigned_to = c.assigned_to,
                assigned_employee = c.assigned_employee != null ? c.assigned_employee.name : null,
                created_by = c.created_by,
                created_at = c.created_at
            };
        }

        /// <summary>
        /// Func - Takes a Claim and maps it to a Claim dto
        /// Expression - Represents it as a expression tree so it can be converted to a SQL query
        /// </summary>
        private static Expression<Func<Claim, ClaimDto>> MapToClaimDto = c => new ClaimDto
        {
            claim_id = c.claim_id,
            policy_id = c.policy_id,
            claim_number = c.claim_number,
            claim_policy = c.claim_policy,
            status = c.status,
            date_of_loss = c.date_of_loss,
            date_reported = c.date_reported,
            reserve_amount = c.reserve_amount,
            paid_amount = c.paid_amount,
            memo = c.memo,
            assigned_to = c.assigned_to,
            assigned_employee = c.assigned_employee != null ? c.assigned_employee.name : null,
            created_by = c.created_by,
            created_at = c.created_at
        };
    }
}


