using FalveyInsuranceGroup.Db;
using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Backend.Models;

namespace FalveyInsuranceGroup.Backend.Services
{
    /// <summary>
    /// The business logic for operations on accounts
    /// </summary>
    public class AccountService
    {
        private readonly FalveyInsuranceGroupContext _context;

        public AccountService(FalveyInsuranceGroupContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new user and adds it to the db
        /// </summary>
        /// <param name="acc_email">The email to be tied with user</param>
        /// <param name="password">The password to be tied with user</param>
        /// <param name="id">Used to fetch the Employee to be tied with user</param>
        /// <returns>A bool value that indicates whether a new user was created or not</returns>
        public async Task<bool> CreateUser(string acc_email, string password, int id)
        {
            if (await IsEmailTaken(acc_email) || await IsIdTaken(id))
            {
                return false;
            }

            // fetches an employee by a given ID and checks to see if they exist
            var employee = await GetEmployee(id);
            if (employee == null)
            {
                return false;
            }
            
            // hashes the given password so that it can be added to the db
            var hashed_pass = BCrypt.Net.BCrypt.EnhancedHashPassword(password); 
            var to_add = new User
            {

                email = acc_email,
                password_hash = hashed_pass,
                role = AssignUserRole(employee.title),
                employee_id = id

            };

            // Adds the new User into the db
            _context.Users.Add(to_add);
            await _context.SaveChangesAsync();

            return true;
        }


        /// <summary>
        /// Checks to see if an email is taken by another user
        /// </summary>
        /// <param name="new_email">The email to check</param>
        /// <returns>A bool value indicating if email is taken</returns>
        private async Task<bool> IsEmailTaken(string new_email)
        {

            return await _context.Users.AnyAsync(u => u.email == new_email);


        }


        /// <summary>
        /// Checks to see if an employee ID is taken by another user
        /// </summary>
        /// <param name="id">The employee ID to check</param>
        /// <returns>A bool value indicating if ID is being used</returns>
        private async Task<bool> IsIdTaken(int id)
        {

            return await _context.Users.AnyAsync(u => u.employee_id == id);

        }


        /// <summary>
        /// Fetches an employee via their ID
        /// </summary>
        /// <param name="id">ID used to fetch the employee</param>
        /// <returns>An Employee object</returns>
        private async Task<Employee?> GetEmployee(int id)
        {
            return await _context.Employees.FirstOrDefaultAsync(u => u.employee_id == id);
        }
        

        /// <summary>
        /// Maps an employees title to their appropiate role in the system
        /// </summary>
        /// <param name="title">Tile used to determine their role</param>
        /// <returns>A string value of their role</returns>
        private string AssignUserRole(string title)
        {
            if (title == "CEO" || title == "IT Specialist")
            {
                return "Admin";
            }
            else if (title == "Manager")
            {
                return title;
            }
            else
            {
                return "Standard";
            }
        }
    }


    

}
