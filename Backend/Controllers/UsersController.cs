using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Db;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Services;
using System.Linq.Expressions;

namespace FalveyInsuranceGroup.Backend.Controllers
{

    /// <summary>
    /// A controller that uses http methods to modify and fetch from a database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;
        private static readonly  string[] ALLOWED_ROLE = {"Standard","Admin","Manager"};

        public UsersController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// <summary>
        /// Gets a list of Users
        /// </summary>
        /// <returns>A list of Users</returns>
        [HttpGet]
        public async Task<List<UserDto>> GetUsers()
        {
            var list_users = await _context.Users
            .AsNoTracking()
            .Select(mapToUserDto)
            .ToListAsync();

            return list_users;
        }


        /// <summary>
        /// Fetches a User by ID
        /// </summary>
        /// <param name="id">The ID of the User to retrieve</param>
        /// <returns>The User specified by the ID</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null) {
                return NotFound($"The User with the ID {id} could not be found");
            }


            return createUserDto(user);
        }

        /// <summary>
        /// Add a User to the database
        /// </summary>
        /// <param name="dto">The dto's values will be transferred to the new User</param>
        /// <returns>A 201 message with a location URL to the new User</returns>
        /// <response code="400">Bad request due to bad input</response>
        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] UserDto dto)
        {
            if (!_service.hasValidEnumType(ALLOWED_ROLE, dto.role))  {
                return BadRequest("Not a valid value for role");
            }

            // checks to see if newUser has email already being used by another User
            if (checkEmail(dto.email)) {
                return BadRequest("That email is already used.");
            }



            if (!await validEmployeeId(dto.employee_id)) {
                return BadRequest("Employee ID is not valid"); 
            }


            // sets the values from the dto into the new User object
            var new_user = new User {
                email = dto.email,
                password_hash = dto.password_hash,
                role = dto.role,
                employee_id = dto.employee_id,
                is_active = dto.is_active,
            };

            _context.Users.Add(new_user);
            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(GetUser), new { id = new_user.user_id }, new_user);

        }

        /// <summary>
        /// Fetches a User by ID and updates its values with the dto's values
        /// </summary>
        /// <param name="id">The user to fetch</param>
        /// <param name="dto">The dto's values will update the User's values</param>
        /// <returns>No content</returns>
        /// <response code="400">Bad request due to bad input</response>
        /// <response code="404">User not found</response>
        /// <response code="204">No content</response>
        [HttpPut("{id}")]
        public async Task<ActionResult<User>> PutUser(int id, [FromBody] UserDto dto)
        {
            if (!_service.hasValidEnumType(ALLOWED_ROLE, dto.role))  {
                return BadRequest("Not a valid value for role");
            }

            // checks to see if the email is already used by another User
            if (checkEmail(dto.email)) {
                return BadRequest("That email is already used.");
            }


            if (!await validEmployeeId(dto.employee_id)) {
                    return BadRequest("Employee ID is not valid"); 
            }

            var update_user = await _context.Users.FindAsync(id);
            if (update_user == null) {
                return NotFound($"The User with the ID {id} was not found");
            }


            // update the fetched User entity
            update_user.email = dto.email;
            update_user.password_hash = dto.password_hash;
            update_user.role = dto.role;
            update_user.employee_id = dto.employee_id;
            update_user.is_active = dto.is_active;
            update_user.updated_at = DateTime.Now;
            await _context.SaveChangesAsync();


            return NoContent();
        }

        /// <summary>
        /// Fetches a User by ID to delete from the database
        /// </summary>
        /// <param name="id">ID used to fetch User</param>
        /// <returns>No content</returns>
        /// <response code="404">User not found</response>
        /// <response code="204">No content</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var delete_user = await _context.Users.FindAsync(id);
            if (delete_user == null) {
                return NotFound($"The User with ID {id} does not exist");
            }

            _context.Users.Remove(delete_user);
            await _context.SaveChangesAsync();


            return NoContent();
        }


        private async Task<bool> validEmployeeId(int user_employ_id)
        {
            var employ_exists = await _context.Employees.AnyAsync(e => e.employee_id == user_employ_id);

            // checks if an employee does not exist
            if (!employ_exists)
            {
                return false;
            }

            var id_taken = await _context.Users.AnyAsync(u => u.employee_id == user_employ_id);

            // checks to see if a user already has the given ID
            if (id_taken)
            {
                return false;
            }

            return true;

        }

        private Boolean checkEmail(string new_email)
        {

            // assigns variable with an IQueryable that contains a User with the same email value as newUser
            // assigns empty IQueryable if no User with email value is found
            var entityWithEmail = _context.Users.Where(u => u.email == new_email);

            // checks to see if IQueryable is empty
            if (entityWithEmail.Any()) {
                return true;
            }

            return false;

        }

        /// <summary>
        /// Takes a User and maps it to a UserDto
        /// Then it represents it as data so it can be converted to a SQL query
        /// </summary>
        private Expression<Func<User, UserDto>> mapToUserDto = u => new UserDto
        {
            user_id = u.user_id,
            email = u.email,
            password_hash = u.password_hash,
            role = u.role,
            employee_id = u.employee_id,
            is_active = u.is_active,
            created_at = u.created_at,
            updated_at = u.updated_at
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="old_user"></param>
        /// <returns>A new UserDto entity</returns>
        private UserDto createUserDto(User old_user)
        {
            return new UserDto {
                user_id = old_user.user_id,
                email = old_user.email,
                password_hash = old_user.password_hash,
                role = old_user.role,
                employee_id = old_user.employee_id,
                is_active = old_user.is_active,
                created_at = old_user.created_at,
                updated_at = old_user.updated_at

            };
        }

    }

}

