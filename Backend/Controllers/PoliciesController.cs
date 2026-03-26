using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    /// Handles operations related to policy data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PoliciesController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;
        private static readonly string[] ALLOWED_TYPE = { "Auto", "Property", "Liability", "Commercial", "Marine", "Other" };
        private static readonly string[] ALLOWED_STATUS = { "Active", "Pending", "Cancelled", "Expired" };

        public PoliciesController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// GET: api/policies

        /// <summary>
        /// Gets a list of policies
        /// </summary>
        /// <returns>A list of policies</returns>
        [HttpGet]
        public async Task<List<PolicyDto>> getPolicies()
        {
            var policy = await _context.Policies
                .Include(p => p.customer) // linked customer
                .Include(p => p.manager)  // linked manager
                .Select(p => new PolicyDto
                {
                    policy_id = p.policy_id,
                    account_number = p.account_number,
                    customer_id = p.customer_id,
                    customer_name = p.customer != null ? p.customer.name : null,
                    manager_id = p.manager_id,
                    manager_name = p.manager != null ? p.manager.name : null,
                    policy_type = p.policy_type,
                    status = p.status,
                    start_date = p.start_date,
                    end_date = p.end_date,
                    exposure_amount = p.exposure_amount,
                    created_at = p.created_at,
                    location_id = p.location_id,
                    location_address = p.location != null ? p.location.address : null,
                }).
            ToListAsync();
            return policy;
        }

        /// GET: api/policies/id

        /// <summary>
        /// Gets a policy by ID
        /// </summary>
        /// <param name="id"> The id of the policy to retrieve</param>
        /// <returns>A policy dto based on provided ID</returns>
        /// <response code="200">Returns the policy</response>
        /// <response code="404">If the policy is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<PolicyDto>> getPolicyById(int id)
        {
            var policy = await _context.Policies.Where(p => p.policy_id == id).FirstOrDefaultAsync();

            // Verify policy exists.
            if (policy == null)
            {
                return NotFound($"Policy with ID {id} not found");
            }

            return createPolicyDto(policy);
        }


        /// GET: api/policies/search?location_id

        /// <summary>
        /// Gets a list of policies based on a Location ID query
        /// </summary>
        /// <returns>A list of policies based on a Location ID query</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<PolicyDto>>> getPoliciesByQuery([FromQuery] int? location_id)
        {
            var policy = _context.Policies
                .Include(p => p.customer) // linked customer
                .Include(p => p.manager)  // linked manager
                .Select(p => new PolicyDto
                {
                    policy_id = p.policy_id,
                    account_number = p.account_number,
                    customer_id = p.customer_id,
                    customer_name = p.customer != null ? p.customer.name : "Unknown",
                    manager_id = p.manager_id,
                    manager_name = p.manager != null ? p.manager.name : "Unknown",
                    policy_type = p.policy_type,
                    status = p.status,
                    start_date = p.start_date,
                    end_date = p.end_date,
                    exposure_amount = p.exposure_amount,
                    created_at = p.created_at,
                    location_id = p.location_id,
                    location_address = p.location != null ? p.location.address : "Unknown"
                });

            if (location_id.HasValue)
            {
                policy = policy.Where(p => p.location_id == location_id.Value);
            }

            var results = await policy.ToListAsync();
            return Ok(results);
        }

        /// POST: api/policies

        /// <summary>
        /// Adds a new policy
        /// </summary>
        /// <param name="new_policy">Policy object to add</param>
        /// <returns>The new record</returns>
        /// <response code="201">Record added successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        public async Task<ActionResult> addPolicy([FromBody] PolicyDto dto)
        {

            // Check if Customer ID exists in system
            if (!await _service.checkCustomer(dto.customer_id))
            {
                return BadRequest("The customer does not exist.");
            }

            // Check if Employee ID exists in system
            if (!await _service.checkEmployee(dto.manager_id))
            {
                return BadRequest("The employee does not exist.");
            }

            // Validate policy type.
            if (!_service.hasValidEnumType(ALLOWED_TYPE, dto.policy_type))
            {
                return BadRequest(new { error = "Please enter a valid policy type." });
            }

            // Validate policy status.
            if (!_service.hasValidEnumType(ALLOWED_STATUS, dto.status))
            {
                return BadRequest(new { error = "Please enter a valid status." });
            }

            var new_policy = new Policy
            {
                policy_id = dto.policy_id,
                account_number = dto.account_number,
                customer_id = dto.customer_id,
                manager_id = dto.manager_id,
                policy_type = dto.policy_type,
                status = dto.status,
                start_date = dto.start_date,
                end_date = dto.end_date,
                exposure_amount = dto.exposure_amount,
                created_at = dto.created_at,
                location_id = dto.location_id
            };

            _context.Policies.Add(new_policy);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(getPolicyById), new { id = new_policy.policy_id }, createPolicyDto(new_policy));
        }

        /// PUT: api/policies/id

        /// <summary>
        ///  Updates an existing policy by ID
        /// </summary>
        /// <param name="id">The policy to update</param>
        /// <param name="updated_policy">Policy object that holds the new details</param>
        /// <returns>No content</returns>
        /// <response code="204">The update was a success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Record not found</response>
        [HttpPut("{id}")]
        public async Task<ActionResult> updatePolicy(int id, [FromBody] PolicyDto updated_policy)
        {
            var policy = await _context.Policies.FindAsync(id);
            
            // Check if Customer ID exists in system
            if (!await _service.checkCustomer(updated_policy.customer_id))
            {
                return BadRequest("The customer does not exist.");
            }

            // Check if Employee ID exists in system
            if (!await _service.checkEmployee(updated_policy.manager_id))
            {
                return BadRequest("The employee does not exist.");
            }

            // Check if Location ID exists in system
            if (!await _service.checkLocation(updated_policy.location_id))
            {
                return BadRequest("The employee does not exist.");
            }

            // Validate policy type.
            if (!_service.hasValidEnumType(ALLOWED_TYPE, updated_policy.policy_type))
            {
                return BadRequest(new { error = "Please enter a valid policy type." });
            }

            // Validate policy status.
            if (!_service.hasValidEnumType(ALLOWED_STATUS, updated_policy.status))
            {
                return BadRequest(new { error = "Please enter a valid status." });
            }

            // Validate created at DateTime.
            if (updated_policy.created_at > DateTime.Now)
            {
                return BadRequest("Creation date cannot be future.");
            }

            // Validate policy is not null.
            if (policy == null)
            {
                return NotFound($"Policy with ID {id} not found");
            }

            // Update fields
            policy.account_number = updated_policy.account_number;
            policy.customer_id = updated_policy.customer_id;
            policy.manager_id = updated_policy.manager_id;
            policy.policy_type = updated_policy.policy_type;
            policy.status = updated_policy.status;
            policy.start_date = updated_policy.start_date;
            policy.end_date = updated_policy.end_date;
            policy.exposure_amount = updated_policy.exposure_amount;
            policy.created_at = updated_policy.created_at;
            policy.location_id = updated_policy.location_id;

            await _context.SaveChangesAsync();

            return Ok(policy);
        }

        /// DELETE: api/policies/id

        /// <summary>
        /// Deletes an existing policy
        /// </summary>
        /// <param name="id">The policy to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">Deletion was a success</response>
        /// <response code="404">Policy not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> deletePolicy(int id)
        {
            var policy = await _context.Policies.FindAsync(id);

            // Check if policy exists.
            if (policy == null)
            {
                return NotFound($"Policy with ID {id} not found");
            }

            _context.Policies.Remove(policy);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Creates a dto using a policy entity model
        /// </summary>
        /// <param name="p">The policy entity model</param>
        /// <returns>A policy dto with the only necessary information</returns>
        private PolicyDto createPolicyDto(Policy p)
        {
            return new PolicyDto
            {
                policy_id = p.policy_id,
                account_number = p.account_number,
                customer_id = p.customer_id,
                customer_name = p.customer != null ? p.customer.name : null,
                manager_id = p.manager_id,
                manager_name = p.manager != null ? p.manager.name : null,
                policy_type = p.policy_type,
                status = p.status,
                start_date = p.start_date,
                end_date = p.end_date,
                exposure_amount = p.exposure_amount,
                created_at = p.created_at,
                location_id = p.location_id,
                location_address = p.location != null ? p.location.address : null
            };
        }
    }
}
