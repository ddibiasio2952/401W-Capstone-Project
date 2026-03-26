using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FalveyInsuranceGroum.Backend.Controllers
{
    /// <summary>
    /// Handles operations related to memo data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MemosController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;

        public MemosController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// GET: api/memos

        /// <summary>
        /// Gets a list of memos
        /// </summary>
        /// <returns>A list of memos</returns>
        [HttpGet]
        public async Task<List<MemoDto>> getMemos()
        {
            var memo = await _context.Memos
                .Select(m => new MemoDto
                {
                    memo_id = m.memo_id,
                    user_id = m.user_id,
                    policy_id = m.policy_id,
                    memo_text = m.memo_text,
                    created_at = m.created_at
                }).
            ToListAsync();
            return memo;
        }

        /// GET: api/memos/id

        /// <summary>
        /// Gets a memo by ID
        /// </summary>
        /// <param name="id"> The id of the memo to retrieve</param>
        /// <returns>A memo dto based on provided ID</returns>
        /// <response code="200">Returns the memo</response>
        /// <response code="404">If the memo is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<MemoDto>> getMemoById(int id)
        {
            var memo = await _context.Memos.Where(m => m.memo_id == id).FirstOrDefaultAsync();

            // Verify memo exists.
            if (memo == null)
            {
                return NotFound($"Memo with ID {id} not found");
            }

            return createMemoDto(memo);
        }

        /// GET: api/memos/search?policy_id

        /// <summary>
        /// Gets a list of memos based on a Policy ID query
        /// </summary>
        /// <returns>A list of memos based on a Policy ID query</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MemoDto>>> getMemosByPolicy([FromQuery] int? policy_id)
        {
            var memo = _context.Memos
                .Select(r => new MemoDto
                {
                    memo_id = r.memo_id,
                    user_id = r.user_id,
                    policy_id = r.policy_id,
                    memo_text = r.memo_text,
                    created_at = r.created_at
                });

            if (policy_id.HasValue)
            {
                memo = memo.Where(r => r.policy_id == policy_id.Value);
            }

            var results = await memo.ToListAsync();
            return Ok(results);
        }

        /// POST: api/memos

        /// <summary>
        /// Adds a new memo
        /// </summary>
        /// <param name="new_memo">Memo object to add</param>
        /// <returns>The new record</returns>
        /// <response code="201">Record added successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        public async Task<ActionResult> addMemo([FromBody] MemoDto dto)
        {
            if (!await _service.checkPolicy(dto.policy_id))
            {
                return BadRequest("The policy does not exist.");
            }

            // Validate text length.
            if (dto.memo_text.Length > 500)
            {
                return BadRequest("The text exceeds the character limit of 500.");
            }

            var new_memo = new Memo
            {
                memo_id = dto.memo_id,
                user_id = dto.user_id,
                policy_id = dto.policy_id,
                memo_text = dto.memo_text,
                created_at = dto.created_at
            };

            _context.Memos.Add(new_memo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(getMemoById), new { id = new_memo.memo_id }, createMemoDto(new_memo));
        }

        /// PUT: api/memos/id

        /// <summary>
        ///  Updates an existing memo by ID
        /// </summary>
        /// <param name="id">The memo to update</param>
        /// <param name="updated_memo">Memo object that holds the new details</param>
        /// <returns>No content</returns>
        /// <response code="204">The update was a success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Record not found</response>
        [HttpPut("{id}")]
        public async Task<ActionResult> updateMemo(int id, [FromBody] MemoDto updated_memo)
        {
            var memo = await _context.Memos.FindAsync(id);

            // Check if Employee ID exists in system
            if (!await _service.checkPolicy(updated_memo.policy_id))
            {
                return BadRequest("The policy does not exist.");
            }

            // Validate created at DateTime.
            if (updated_memo.created_at > DateTime.Now)
            {
                return BadRequest("Creation date cannot be future.");
            }

            // Validate memo is not null.
            if (memo == null)
            {
                return NotFound($"Memo with ID {id} not found");
            }

            // Validate text length.
            if (updated_memo.memo_text.Length > 500)
            {
                return BadRequest("The text exceeds the character limit of 500.");
            }

            // Update fields
            memo.user_id = updated_memo.user_id;
            memo.policy_id = updated_memo.policy_id;
            memo.memo_text = updated_memo.memo_text;
            memo.created_at = updated_memo.created_at;

            await _context.SaveChangesAsync();

            return Ok(memo);
        }

        /// DELETE: api/memos/id

        /// <summary>
        /// Deletes an existing memo
        /// </summary>
        /// <param name="id">The memo to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">Deletion was a success</response>
        /// <response code="404">Memo not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteMemo(int id)
        {
            var memo = await _context.Memos.FindAsync(id);

            // Check if memo exists.
            if (memo == null)
            {
                return NotFound($"Memo with ID {id} not found");
            }

            _context.Memos.Remove(memo);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        /// <summary>
        /// Creates a dto using a memo entity model
        /// </summary>
        /// <param name="p">The memo entity model</param>
        /// <returns>A memo dto with the only necessary information</returns>
        private MemoDto createMemoDto(Memo m)
        {
            return new MemoDto
            {
                memo_id = m.memo_id,
                user_id = m.user_id,
                policy_id = m.policy_id,
                memo_text = m.memo_text,
                created_at = m.created_at,
            };
        }
    }
}
