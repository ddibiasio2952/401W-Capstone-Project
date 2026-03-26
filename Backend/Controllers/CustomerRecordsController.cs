using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Services;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    /// Handles operations related to customer documents
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerRecordsController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;
       private static readonly string[] ALLOWED_TYPES = { "Customer", "Policy", "Claim" };

        public CustomerRecordsController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// GET: api/customerrecords

        /// <summary>
        /// Gets a list of records
        /// </summary>
        /// <returns>A list of records</returns>
        [HttpGet]
        public async Task<ActionResult<List<CustomerRecordDto>>> getRecords()
        {
            var records = await _context.CustomerRecords
            .AsNoTracking()
            .Include(r => r.employee_uploader)
            .Select(r => new CustomerRecordDto
            {
                document_id = r.document_id,
                file_name = r.file_name,
                url = r.url,
                uploaded_by = r.uploaded_by,
                uploaded_by_name = r.employee_uploader != null ? r.employee_uploader.name : null,
                uploaded_at = r.uploaded_at,
                attached_to_type = r.attached_to_type,
                description = r.description
            }).ToListAsync();


            return Ok(records);
        }

        /// GET: api/customerrecords/id

        /// <summary>
        /// Gets a record by ID
        /// </summary>
        /// <param name="id"> The id of the record to retrieve</param>
        /// <returns>A customer record dto based on provided ID</returns>
        /// <response code="200">Returns the record</response>
        /// <response code="404">If the record is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerRecordDto>> getRecord(int id)
        {
            var record = await _context.CustomerRecords
            .AsNoTracking()
            .Include(r => r.employee_uploader)
            .FirstOrDefaultAsync(r => r.document_id == id);;

            if (record == null)
            {
                return NotFound($"Record with ID {id} not found");
            }

            return Ok(createCustomerRecordDto(record));
        }

        /// POST: api/customerrecords

        /// <summary>
        /// Adds a new record
        /// </summary>
        /// <param name="record">Customer record object to add</param>
        /// <returns>The new record</returns>
        /// <response code="200">Record added successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        public async Task<ActionResult<CustomerRecordDto>> addRecord(CustomerRecord record)
        {
             if (!_service.hasValidEnumType(ALLOWED_TYPES, record.attached_to_type)) {
                return BadRequest("Invalid type input");
            }

            _context.CustomerRecords.Add(record);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(getRecord), new { id = record.document_id }, createCustomerRecordDto(record));
        }

        /// PUT: api/customerrecords/id

        /// <summary>
        ///  Updates an existing record by ID
        /// </summary>
        /// <param name="id">The record to update</param>
        /// <param name="updated_record">Record object that holds the new details</param>
        /// <returns>No content</returns>
        /// <response code="204">The update was a success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Record not found</response>
        [HttpPut("{id}")]
        public async Task<ActionResult> updateRecord(int id, CustomerRecord updated_record)
        {
            if (!_service.hasValidEnumType(ALLOWED_TYPES, updated_record.attached_to_type)) {
                return BadRequest("Invalid type input");
            }

            var existing_record = await _context.CustomerRecords.FindAsync(id);

            // Checks to see if customer record exists
            if (existing_record == null) {
                return NotFound($"Record with ID {id} not found");
            }

            // Updates fields
            existing_record.file_name = updated_record.file_name;
            existing_record.url = updated_record.url;
            existing_record.uploaded_by = updated_record.uploaded_by;
            existing_record.uploaded_at = updated_record.uploaded_at;
            existing_record.attached_to_type = updated_record.attached_to_type;
            existing_record.attached_to_id = updated_record.attached_to_id;
            existing_record.description = updated_record.description;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// DELETE: api/customerrecords/id

        /// <summary>
        /// Deletes an existing record
        /// </summary>
        /// <param name="id">The record to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">Deletion was a success</response>
        /// <response code="404">Record not found</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult> deleteRecord(int id)
        {
            var record = await _context.CustomerRecords.FindAsync(id);

            if (record == null) {
                return NotFound($"Record with ID {id} not found");
            }

            _context.CustomerRecords.Remove(record);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Creates a dto using a CustomerRecord entity model
        /// </summary>
        /// <param name="r">The record entity model</param>
        /// <returns>A customer record dto with the only necessary information</returns>
        private CustomerRecordDto createCustomerRecordDto(CustomerRecord r)
        {
            return new CustomerRecordDto
            {
                document_id = r.document_id,
                file_name = r.file_name,
                url = r.url,
                uploaded_by = r.uploaded_by,
                uploaded_by_name = r.employee_uploader != null ? r.employee_uploader.name : null,
                uploaded_at = r.uploaded_at,
                attached_to_type = r.attached_to_type,
                description = r.description
            };
        }

    }
}
