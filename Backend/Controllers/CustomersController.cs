using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    /// Handles operations related to customer data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;
        public CustomersController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// GET: api/customers

        /// <summary>
        /// Gets a list of customers
        /// </summary>
        /// <returns>A list of customers</returns>
        [HttpGet]
        public async Task<List<CustomerDto>> getCustomers()
        {
            var customer = await _context.Customers.Select(c => new CustomerDto
            {
                customer_id = c.customer_id,
                name = c.name,
                email = c.email,
                phone = c.phone,
                addr_line1 = c.addr_line1,
                addr_line2 = c.addr_line2,
                city = c.city,
                state_code = c.state_code,
                zip_code = c.zip_code,
                created_at = c.created_at
            }).
            ToListAsync();
            return customer;
        }

        /// GET: api/customers/id

        /// <summary>
        /// Gets a customer by ID
        /// </summary>
        /// <param name="id"> The id of the customer to retrieve</param>
        /// <returns>A customer dto based on provided ID</returns>
        /// <response code="200">Returns the customer</response>
        /// <response code="404">If the customer is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> getCustomerById(int id)
        {
            var customer = await _context.Customers.Where(c => c.customer_id == id).FirstOrDefaultAsync();

            // Check if customer exists.
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            return createCustomerDto(customer);
        }

        /// POST: api/customers

        /// <summary>
        /// Adds a new customer
        /// </summary>
        /// <param name="dto">Customer object to add</param>
        /// <returns>The new record</returns>
        /// <response code="201">Record added successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        public async Task<ActionResult> addCustomer([FromBody] CustomerDto dto)
        {
            // Refuse user input for customer ID.
            if (dto.customer_id.HasValue)
            {
                return BadRequest("Customer ID should not be provided on creation.");
            }

            // Validate no whitespace in email.
            if (dto.email.Contains(" "))
            {
                return BadRequest(new { error = "Remove whitespace from email." });
            }

            // Validate state code.
            if (!InputService.checkStateCode(dto.state_code))
            {
                return BadRequest(new
                {
                    error =
                    "Please enter a two-letter code for a state, the District of Columbia, or the five US territories."
                });
            }

            // Validate created time.
            if (dto.created_at > DateTime.Now)
            {
                return BadRequest("Creation date cannot be future.");
            }

            var new_customer = new Customer
            {
                customer_id = dto.customer_id,
                name = dto.name,
                email = dto.email,
                phone = dto.phone,
                addr_line1 = dto.addr_line1,
                addr_line2 = dto.addr_line2,
                city = dto.city,
                state_code = dto.state_code,
                zip_code = dto.zip_code,
                created_at = DateTime.Now
            };

            _context.Customers.Add(new_customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(getCustomerById), new { id = new_customer.customer_id }, createCustomerDto(new_customer));
        }

        /// PUT: api/customers/id

        /// <summary>
        ///  Updates an existing customer by ID
        /// </summary>
        /// <param name="id">The customer to update</param>
        /// <param name="updated_customer">Customer object that holds the new details</param>
        /// <returns>No content</returns>
        /// <response code="204">The update was a success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Record not found</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> updateCustomer(int id, CustomerDto updated_customer)
        {
            var customer = await _context.Customers.FindAsync(id);

            // Check if customer exists.
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            // Validate no whitespace in email.
            if (updated_customer.email.Contains(" "))
            {
                return BadRequest(new { error = "Remove whitespace from email." });
            }

            // Validate state code.
            if (!InputService.checkStateCode(updated_customer.state_code))
            {
                return BadRequest(new { error = "Please enter a two-letter code for a state, the District of Columbia, or the five US territories." });
            }

            // Validate created time.
            if (updated_customer.created_at > DateTime.Now)
            {
                return BadRequest("Creation date cannot be future.");
            }

            // Update fields
            customer.customer_id = updated_customer.customer_id;
            customer.name = updated_customer.name;
            customer.email = updated_customer.email;
            customer.phone = updated_customer.phone;
            customer.addr_line1 = updated_customer.addr_line1;
            customer.addr_line2 = updated_customer.addr_line2;
            customer.city = updated_customer.city;
            customer.state_code = updated_customer.state_code;
            customer.zip_code = updated_customer.zip_code;
            customer.created_at = updated_customer.created_at;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// DELETE: api/customers/id

        /// <summary>
        /// Deletes an existing customer
        /// </summary>
        /// <param name="id">The customer to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">Deletion was a success</response>
        /// <response code="404">Customer not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            // Check if customer exists.
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Creates a dto using a customer entity model
        /// </summary>
        /// <param name="c">The customer entity model</param>
        /// <returns>A customer dto with the only necessary information</returns>
        private CustomerDto createCustomerDto(Customer c)
        {
            return new CustomerDto
            {
                customer_id = c.customer_id,
                name = c.name,
                email = c.email,
                phone = c.phone,
                addr_line1 = c.addr_line1,
                addr_line2 = c.addr_line2,
                city = c.city,
                state_code = c.state_code,
                zip_code = c.zip_code,
                created_at = c.created_at
            };
        }

    }
}
