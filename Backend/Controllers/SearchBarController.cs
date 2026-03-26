using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using FalveyInsuranceGroup.Backend.Services;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchBarController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;

        public SearchBarController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// GET: api/searchbar/id

        /// <summary>
        /// Gets a list of policies based on a Customer ID query
        /// </summary>
        /// <returns>A list of policies based on a Customer ID query</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PolicyDto>>> GetPolicies([FromQuery] int? customer_id)
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
                });

            if (customer_id.HasValue)
            {
                policy = policy.Where(p => p.customer_id == customer_id.Value);
            }

            var results = await policy.ToListAsync();
            return Ok(results);
        }

        /// GET: api/searchbar/id

        /// <summary>
        /// Gets a list of memos based on a Policy ID query
        /// </summary>
        /// <returns>A list of memos based on a Policy ID query</returns>
        /*[HttpGet]
        public async Task<ActionResult<IEnumerable<MemoDto>>> GetMemos([FromQuery] int? policy_id)
        {
            var memo = _context.Memos
                .Select(m => new MemoDto
                {
                    memo_id = m.memo_id,
                    user_id = m.user_id,
                    policy_id = m.policy_id,
                    memo_text = m.memo_text,
                    created_at = m.created_at
                });

            if (policy_id.HasValue)
            {
                memo = memo.Where(m => m.policy_id == policy_id.Value);
            }

            var results = await memo.ToListAsync();
            return Ok(results);
        }*/
    }
}
