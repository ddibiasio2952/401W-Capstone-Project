using System.Text.RegularExpressions;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Db;
using Microsoft.EntityFrameworkCore;

namespace FalveyInsuranceGroup.Backend.Services
{
    /// <summary>
    /// General helper methods for backend
    /// </summary>
    public class HelperService
    {
        private readonly FalveyInsuranceGroupContext _context;

        public HelperService(FalveyInsuranceGroupContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get an employee by their ID
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns>A possible employee</returns>
        public async Task<Employee?> GetEmployee(int id)
        {
            return await _context.Employees.FirstOrDefaultAsync(e => e.employee_id == id);

        }

        /// <summary>
        /// Get a user by their ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>A possible user</returns>
        public async Task<User?> GetUser(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.user_id == id);
        }

    }
}
