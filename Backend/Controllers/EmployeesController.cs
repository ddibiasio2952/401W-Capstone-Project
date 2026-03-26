using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Services;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;
        private static readonly string[] ALLOWED_STATUS = { "Active", "Inactive", "Leave", "Terminated" };
        private readonly string[] ALLOWED_TITLE = { "Manager", "Staff", "CEO", "IT Specialist" };
        
        public EmployeesController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// GET: api/employees

        /// <summary>
        /// Gets a list of employees
        /// </summary>
        /// <returns>A list of employees</returns>
        [HttpGet]
        public async Task<List<EmployeeDto>> getEmployees()
        {
            var employees = await _context.Employees
            .AsNoTracking()
            .Select(e => new EmployeeDto
            {
                employee_id = e.employee_id,
                name = e.name,
                title = e.title,
                email = e.email,
                phone = e.phone
            }).
            ToListAsync();

            return employees;
        }

        /// GET: api/employees/id

        /// <summary>
        /// Gets an employee by ID
        /// </summary>
        /// <param name="id"> The id of the employee to retrieve</param>
        /// <returns>An employee dto based on provided ID</returns>
        /// <response code="200">Returns the employee</response>
        /// <response code="404">If the employee is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> getEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return Ok(createEmployeeDto(employee));
        }

        /// POST: api/employees

        /// <summary>
        /// Adds a new employee
        /// </summary>
        /// <param name="employee">Employee object to add</param>
        /// <returns>The new employee</returns>
        /// <response code="200">Employee added successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> addEmployee(Employee employee)
        {
            // Checks to see if all required inputs are provided
            if (!_service.hasValidEnumType(ALLOWED_STATUS, employee.status))
            {
                return BadRequest("Invalid status input");
            }

            if(!_service.hasValidEnumType(ALLOWED_TITLE, employee.title ))
            {
                return BadRequest("Invalid title input");
            }

            // Ensures if a given email is unique
            if (await _service.hasDuplicateEmail<Employee>(employee.email)) {
                return BadRequest("The given email is already in use");
            }

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(getEmployee), new { id = employee.employee_id }, createEmployeeDto(employee));
        }


        /// PUT: api/employees/id

        /// <summary>
        ///  Updates an existing employee by ID
        /// </summary>
        /// <param name="id">The employee to update</param>
        /// <param name="updated_employee">Employee object that holds the new details</param>
        /// <returns>No content</returns>
        /// <response code="204">The update was a success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Employee not found</response>
        [HttpPut("{id}")]
        public async Task<ActionResult> updateEmployee(int id, Employee updated_employee)
        {

            if (!_service.hasValidEnumType(ALLOWED_STATUS, updated_employee.status))
            {
                return BadRequest("Invalid status input");
            }

            if(!_service.hasValidEnumType(ALLOWED_TITLE, updated_employee.title ))
            {
                return BadRequest("Invalid title input");
            }


            // Ensures if a given email is unique
            if (await _service.hasDuplicateEmail<Employee>(updated_employee.email))
            {
                return BadRequest("The given email is already in use");
            }

            var existing_employee = await _context.Employees.FindAsync(id);

            if (existing_employee == null) {
                return NotFound($"Employee with {id} not found");
            }

            // Update fields
            existing_employee.name = updated_employee.name;
            existing_employee.title = updated_employee.title;
            existing_employee.email = updated_employee.email;
            existing_employee.phone = updated_employee.phone;
            existing_employee.status = updated_employee.status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// DELETE: api/employees/id

        /// <summary>
        /// Deletes an existing employee
        /// </summary>
        /// <param name="id">The employee to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">Deletion was a success</response>
        /// <response code="404">Employee not found</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult> deleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null) {
                return NotFound($"Employee with {id} not found");
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Creates a dto using an Employee entity model
        /// </summary>
        /// <param name="e">The employee entity model</param>
        /// <returns>An employee dto with only necessary information</returns>
        private EmployeeDto createEmployeeDto(Employee e)
        {
            return new EmployeeDto
            {
                employee_id = e.employee_id,
                name = e.name,
                title = e.title,
                email = e.email,
                phone = e.phone
            };
        }

    }
}



