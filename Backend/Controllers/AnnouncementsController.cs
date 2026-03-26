using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    /// Handles operations related to announcement data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncementsController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;
        public AnnouncementsController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// GET: api/announcements

        /// <summary>
        /// Gets a list of announcements
        /// </summary>
        /// <returns>A list of announcements</returns>

        [HttpGet]
        public async Task<List<AnnouncementDto>> getAnnouncements()
        {
            var announcement = await _context.Announcements.Select(a => new AnnouncementDto
            {
                announcement_id = a.announcement_id,
                title = a.title,
                body = a.body,
                publish_at = a.publish_at,
                expire_at = a.expire_at,
                created_by = a.created_by,
                created_at = a.created_at,
            }).
            ToListAsync();
            return announcement;
        }

        /// GET: api/announcements/id

        /// <summary>
        /// Gets an announcement by ID
        /// </summary>
        /// <param name="id"> The id of the announcement to retrieve</param>
        /// <returns>An announcement dto based on provided ID</returns>
        /// <response code="200">Returns the announcement</response>
        /// <response code="404">If the announcement is not found</response>
        /// 
        [HttpGet("{id}")]
        public async Task<ActionResult<AnnouncementDto>> getAnnouncementById(int id)
        {
            var announcement = await _context.Announcements
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.announcement_id == id);

            // Check if announcement exists.
            if (announcement == null)
            {
                return NotFound($"Announcement with ID {id} not found");
            }

            return createAnnouncementDto(announcement);
        }

        /// POST: api/announcements

        /// <summary>
        /// Adds a new announcement
        /// </summary>
        /// <param name="dto">Announcement object to add</param>
        /// <returns>The new record</returns>
        /// <response code="201">Record added successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        public async Task<ActionResult> addAnnouncement([FromBody] AnnouncementDto dto)
        {
            // Refuse user input for announcement ID.
            if (dto.announcement_id.HasValue)
            {
                return BadRequest("Announcement ID should not be provided on creation.");
            }

            // Check if only whitespace in title.
            if (InputService.checkWhitespace(dto.title))
            {
                return BadRequest(new { error = "Title cannot be empty." });
            }

            // Check if created by an existing User
            if (!await _service.checkUser(dto.created_by))
            {
                return BadRequest("The user does not exist.");
            }

            // Ensure DateTime is not future.
            if (dto.created_at > DateTime.Now)
            {
                return BadRequest("Creation date cannot be future.");
            }

            var new_announcement = new Announcement
            {
                announcement_id = dto.announcement_id,
                title = dto.title,
                body = dto.body,
                publish_at = dto.publish_at,
                expire_at = dto.expire_at,
                created_by = dto.created_by,
                created_at = DateTime.Now
            };

            _context.Announcements.Add(new_announcement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(getAnnouncementById), new { id = new_announcement.announcement_id }, createAnnouncementDto(new_announcement));
        }

        /// PUT: api/announcements/id

        /// <summary>
        ///  Updates an existing announcement by ID
        /// </summary>
        /// <param name="id">The announcement to update</param>
        /// <param name="updated_announcement">Announcement object that holds the new details</param>
        /// <returns>No content</returns>
        /// <response code="204">The update was a success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Record not found</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> updateannouncement(int id, AnnouncementDto updated_announcement)
        {
            var announcement = await _context.Announcements.FindAsync(id);

            // Check if announcement exists.
            if (announcement == null)
            {
                return NotFound($"Announcement with ID {id} not found");
            }

            // Check if only whitespace in title.
            if (InputService.checkWhitespace(updated_announcement.title))
            {
                return BadRequest(new { error = "Title cannot be empty." });
            }

            // Check if created by an existing User
            if (!await _service.checkUser(updated_announcement.created_by))
            {
                return BadRequest("The user does not exist.");
            }

            // Validate created at DateTime.
            if (updated_announcement.created_at > DateTime.Now)
            {
                return BadRequest("Creation date cannot be future.");
            }

            announcement.title = updated_announcement.title;
            announcement.body = updated_announcement.body;
            announcement.publish_at = updated_announcement.publish_at;
            announcement.expire_at = updated_announcement.expire_at;
            announcement.created_by = updated_announcement.created_by;
            announcement.created_at = updated_announcement.created_at;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// DELETE: api/announcements/id

        /// <summary>
        /// Deletes an existing announcement
        /// </summary>
        /// <param name="id">The announcement to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">Deletion was a success</response>
        /// <response code="404">Announcement not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteAnnouncement(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);

            // Check if announcement exists.
            if (announcement == null)
            {
                return NotFound($"Announcement with ID {id} not found");
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Creates a dto using a announcement entity model
        /// </summary>
        /// <param name="a">The announcement entity model</param>
        /// <returns>An announcement dto with the only necessary information</returns>
        private AnnouncementDto createAnnouncementDto(Announcement a)
        {
            return new AnnouncementDto
            {
                announcement_id = a.announcement_id,
                title = a.title,
                body = a.body,
                publish_at = a.publish_at,
                expire_at = a.expire_at,
                created_by = a.created_by,
                created_at = a.created_at,
            };
        }

    }
}
