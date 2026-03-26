using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Db;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardDataController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;

        public DashboardDataController(FalveyInsuranceGroupContext context)
        {
            _context = context;
        }

        [HttpGet("claims-summary")]
        public async Task<ActionResult<IEnumerable<object>>> GetClaimsSummary()
        {
            var claims = await _context.Claims
                .Select(c => new
                {
                    c.claim_id,
                    c.policy_id,
                    c.claim_number,
                    c.status,
                    c.date_reported
                }).ToListAsync();

            var policies = await _context.Policies
                .Include(p => p.customer)
                .Select(p => new
                {
                    p.policy_id,
                    customer_id = p.customer.customer_id,
                    customer_name = p.customer.name
                }).ToListAsync();

            var data = claims
                .Join(policies,
                    c => c.policy_id,
                    p => p.policy_id,
                    (c, p) => new
                    {
                        ClaimId = c.claim_id,
                        ClaimNumber = c.claim_number ?? $"CLM-{c.claim_id}",
                        InsuredName = p.customer_name,
                        CustomerId = p.customer_id,
                        Status = c.status,
                        DateReported = c.date_reported
                    }).ToList();

            return Ok(data);
        }
    }
}
