using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Db;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Dtos;
using System.Linq.Expressions;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    /// handles operations related to claim note data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimNotesController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;

        public ClaimNotesController(FalveyInsuranceGroupContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Gets a list of claim notes
        /// </summary>
        /// <returns>A list of claim notes</returns>
        [HttpGet]
        public async Task<List<ClaimNoteDto>> GetClaimNotes()
        {
            var listClaimNotes = await _context.ClaimNotes
            .AsNoTracking()
            .Select(MapToClaimNoteDto)
            .ToListAsync();

            return listClaimNotes;

        }


        /// <summary>
        /// Gets a claim note by ID
        /// </summary>
        /// <param name="id">The ID of the claim note to retreive</param>
        /// <returns>A claim note dto based on provided ID</returns>
        /// <response code="200">Returns the claim note</response>
        /// <response code="404">If the claim note is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClaimNoteDto>> GetClaimNote(int id)
        {
            var fetchClaimNote = await _context.ClaimNotes
            .AsNoTracking()
            .Include(c => c.assigned_claim)
            .Include(c => c.user_creator)
            .FirstOrDefaultAsync(c => c.note_id == id);


            if (fetchClaimNote == null) {
                return NotFound($"Claim note with ID {id} not found");
            }

            return createClaimNoteDto(fetchClaimNote);

        }


        /// <summary>
        /// adds a new claim note
        /// </summary>
        /// <param name="dto">ClaimNote object to add</param>
        /// <returns>The new record</returns>
        /// <response code="200">Record added successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        public async Task<ActionResult> AddClaimNote([FromBody] ClaimNoteDto dto)
        {
            if (dto.author_user_id != null) {
                if (!await hasValidUserId(dto.author_user_id)) {
                    return BadRequest("The given user ID does not exist");
                }
            }

            if (!await hasValidClaimId(dto.claim_id)) {
                return BadRequest("The given claim ID does not exist");
            }


                var new_claim_note = new ClaimNote
            {
                claim_id = dto.claim_id,
                author_user_id = dto.author_user_id,
                note_text = dto.note_text
            };

            _context.ClaimNotes.Add(new_claim_note);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClaimNote), new { id = new_claim_note.note_id },
                                          createClaimNoteDto(new_claim_note));
        }


        /// <summary>
        /// Updates a claim note from the database
        /// </summary>
        /// <param name="id">The ID of the claim note to retrieve</param>
        /// <param name="dto">Dto's values will overwrite the retrieved claim note's values</param>
        /// <returns>No content</returns>
        /// <response="204">Update successful</response>
        /// <response="400">Validation failed</response>
        /// <response="404">Claim note entity not found</response>
        [HttpPut("{id}")]
        public async Task<ActionResult<ClaimNote>> UpdateClaimNote(int id, [FromBody] ClaimNoteDto dto)
        {
            var claim_note = await _context.ClaimNotes.FindAsync(id);

            // checks to see if the fetched entity exists
            if (claim_note == null)
            {
                return NotFound();
            }


            // updates field and saves changes to the database
            claim_note.claim_id = dto.claim_id;
            claim_note.author_user_id = dto.author_user_id;
            claim_note.note_text = dto.note_text;
            await _context.SaveChangesAsync();


            return NoContent();
        }


        /// <summary>
        /// Deletes a claim note from the database
        /// </summary>
        /// <param name="id">The ID of the claim note to delete</param>
        /// <returns>No content</returns>
        /// <response="204"> Deletion successful</response>
        /// <response="404"> Entity not found</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ClaimNote>> DeleteClaimNote(int id)
        {
            var noteToDelete = await _context.ClaimNotes.FindAsync(id);

            if (noteToDelete == null)
            {
                return NotFound($"Claim with ID {id} not found");
            }

            _context.ClaimNotes.Remove(noteToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Checks to see if a User entity contains the same ID as the given ID parameter
        /// </summary>
        /// <param name="user_id"></param>
        /// <returns>A task with bool result</returns>
        private async Task<bool> hasValidUserId(int? author_user_id)
        {
            return await _context.Users.AnyAsync(u => u.user_id == author_user_id);

        }

        /// <summary>
        /// Checks to see if a ClaimNote's claim ID links to an existing Claim
        /// </summary>
        /// <param name="note_claim_id">Holds the ClaimNote's claim ID</param>
        /// <returns>A Task with bool result</returns>
        private async Task<bool> hasValidClaimId(int note_claim_id)
        {
            return await _context.Claims.AnyAsync(c => c.claim_id == note_claim_id);

        }

        /// <summary>
        /// Creates a dto using a claim note entity model
        /// </summary>
        /// <param name="c">The claim note entity model</param>
        /// <returns>A claim note dto with the only necessary information</returns>
        private ClaimNoteDto createClaimNoteDto(ClaimNote c)
        {
            return new ClaimNoteDto
            {
                note_id = c.note_id,
                claim_id = c.claim_id,
                author_user_id = c.author_user_id,
                note_text = c.note_text,
                created_at = c.created_at,
                assigned_claim = c.assigned_claim != null ? c.assigned_claim.claim_number : null

            };

        }

        /// <summary>
        /// Takes a ClaimNote and maps it to a ClaimNoteDto
        /// Then it represents it as data so it can be converted to a SQL query
        /// </summary>
        private static Expression<Func<ClaimNote, ClaimNoteDto>> MapToClaimNoteDto = c => new ClaimNoteDto
        {
            note_id = c.note_id,
            claim_id = c.claim_id,
            author_user_id = c.author_user_id,
            note_text = c.note_text,
            created_at = c.created_at,
            assigned_claim = c.assigned_claim != null ? c.assigned_claim.claim_number : null

        };

    }




}

