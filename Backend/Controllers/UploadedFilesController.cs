using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    ///  Handles operations related to file data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UploadedFilesController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;
        private static readonly string[] ALLOWED_TYPES = { "jpg", "jpeg", "gif", "pdf", "doc", "docx" };
        private static readonly long MAX_SIZE = 16 * 1024 * 1024;
        public UploadedFilesController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// POST: api/uploadedfiles/upload

        /// <summary>
        /// Adds a new file
        /// </summary>
        /// <param name="fileInput">File object to add</param>
        /// <returns>The message confirming upload</returns>
        /// <response code="200">File added successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(List<IFormFile> fileInput, [FromForm] UploadedFileDto dto)
        {
            foreach (var file in fileInput)
            {
                // Input validation

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant().TrimStart('.');
                var mime = _service.hasValidFileExtension(extension, ALLOWED_TYPES);
                long size = file.Length;
                var memory_stream = new MemoryStream();

                if (file == null)
                {
                    return BadRequest("No file to upload.");
                }

                if (size > MAX_SIZE)
                {
                    return BadRequest("File exceeds size limit of 16 MB.");
                }

                if (await _service.hasDuplicateFileName(file.FileName))
                {
                    return BadRequest("Duplicate file name detected. Please upload a file with a different name.");
                }

                try
                {
                    // Sends bytes to 
                    await file.CopyToAsync(memory_stream);

                    var new_file = new UploadedFile
                    {
                        file_name = Path.GetFileName(file.FileName),
                        file_data = memory_stream.ToArray(),
                        media_type = mime,
                        user_id = dto.user_id,
                        customer_id = dto.customer_id,
                        policy_id = dto.policy_id,
                        created_at = DateTime.Now
                    };

                    _context.UploadedFiles.Add(new_file);

                    await _context.SaveChangesAsync();
                }
                catch
                {
                    return BadRequest("File failed to upload.");
                }
            }
            return Ok(new { text = "File uploaded successfully." });
        }

        /// GET: api/uploadedfiles/id

        /// <summary>
        /// Gets a file by ID
        /// </summary>
        /// <param name="id"> The id of the file to retrieve</param>
        /// <returns>A file based on provided ID</returns>
        /// <response code="200">Returns the file</response>
        /// <response code="404">If the file is not found</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> getFileById(int id)
        {
            var file = await _context.UploadedFiles.Where(f => f.file_id == id).FirstOrDefaultAsync();

            // Verify file exists.
            if (file == null)
            {
                return NotFound($"File with ID {id} not found");
            }

            return File(file.file_data, file.media_type, file.file_name);
        }

        /// GET: api/uploadedfiles/policysearch?policy_id

        /// <summary>
        /// Gets a list of files based on a Policy ID query
        /// </summary>
        /// <returns>A list of files based on a Policy ID query</returns>
        [HttpGet("policysearch")]
        public async Task<ActionResult<IEnumerable<UploadedFileDto>>> getFilesByPolicy([FromQuery] int? policy_id)
        {
            var file = _context.UploadedFiles
                .Select(f => new UploadedFileDto
                {
                    file_id = f.file_id,
                    file_name = f.file_name,
                    media_type = f.media_type,
                    user_id = f.user_id,
                    customer_id = f.customer_id,
                    policy_id = f.policy_id,
                    created_at = f.created_at,
                });

            if (policy_id.HasValue)
            {
                file = file.Where(f => f.policy_id == policy_id.Value);
            }

            var results = await file.ToListAsync();
            return Ok(results);
        }

        /// GET: api/uploadedfiles/customersearch?customer_id

        /// <summary>
        /// Gets a list of files based on a Customer ID query
        /// </summary>
        /// <returns>A list of files based on a Customer ID query</returns>
        [HttpGet("customersearch")]
        public async Task<ActionResult<IEnumerable<UploadedFileDto>>> getFilesByCustomer([FromQuery] int? customer_id)
        {
            var file = _context.UploadedFiles
                .Select(f => new UploadedFileDto
                {
                    file_id = f.file_id,
                    file_name = f.file_name,
                    media_type = f.media_type,
                    user_id = f.user_id,
                    customer_id = f.customer_id,
                    policy_id = f.policy_id,
                    created_at = f.created_at,
                });

            if (customer_id.HasValue)
            {
                file = file.Where(f => f.customer_id == customer_id.Value);
            }

            var results = await file.ToListAsync();
            return Ok(results);
        }

        /// DELETE: api/uploadedfiles/id

        /// <summary>
        /// Deletes an existing file
        /// </summary>
        /// <param name="id">The file to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">Deletion was a success</response>
        /// <response code="404">File not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteFile(int id)
        {
            var file = await _context.UploadedFiles.FindAsync(id);

            // Check if file exists.
            if (file == null)
            {
                return NotFound($"File with ID {id} not found");
            }

            _context.UploadedFiles.Remove(file);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Creates a dto using a file entity model
        /// </summary>
        /// <param name="f">The file entity model</param>
        /// <returns>A file dto with the only necessary information</returns>
        private UploadedFileDto createFileDto(Models.UploadedFile f)
        {
            return new UploadedFileDto
            {
                file_id = f.file_id,
                file_name = f.file_name,
                media_type = f.media_type,
                user_id = f.user_id,
                customer_id = f.customer_id,
                policy_id = f.policy_id,
                created_at = f.created_at,
            };
        }
    }
}
