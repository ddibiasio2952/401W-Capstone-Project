using Microsoft.EntityFrameworkCore;
using FalveyInsuranceGroup.Db;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace FalveyInsuranceGroup.Backend.Services
{
    /// <summary>
    /// General input validation methods for backend
    /// </summary>
    public class InputService
    {
        private readonly FalveyInsuranceGroupContext _context;
        public InputService(FalveyInsuranceGroupContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checks to see if the version number follows semantic versioning
        /// </summary>
        /// <param name="version">The version number</param>
        /// <returns>
        ///     True - Has a valid format
        ///     False - Has an invalid format
        /// </returns>
        public bool hasValidVersion(string version)
        {
            // Format is v[MAJOR].[MINOR].[PATCH]
            var version_regex = @"^v\d+\.\d+\.\d+$";

            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            version = version.Trim();

            return Regex.IsMatch(version, version_regex);
        }

        /// <summary>
        /// Checks to see if a entity object holds a valid policy
        /// </summary>
        /// <param name="policy_id">The id of a policy/param>
        /// <returns>
        ///     True - Has a valid policy
        ///     False - Has an invalid policy
        /// </returns>
        public async Task<bool> hasValidPolicy(int policy_id)
        {
            return await _context.Policies.AnyAsync(p => p.policy_id == policy_id);
        }

        /// <summary>
        /// Checks to see if an entity object holds a valid enum type
        /// </summary>
        /// <param name="type">The enum type of entity</param>
        /// <returns>
        ///     True - Has a valid enum type
        ///     False - Has an invalid enum type
        /// </returns>
        public bool hasValidEnumType(string[] allowed_types, string type)
        {
            return allowed_types.Contains(type);
        }

        /// <summary>
        ///  Checks to see if a claim has a duplicate claim number
        /// </summary>
        /// <param name="claim_number">The claim number to check</param>
        /// <returns>
        ///     True - has a duplicate claim number
        ///     False - Has a unique claim number
        /// </returns>
        public async Task<bool> hasDuplicateClaimNumber(string claim_number)
        {

            return await _context.Claims.AnyAsync(c => c.claim_number == claim_number);
        }

        /// <summary>
        ///  Checks to see if entity model has a duplicate email
        /// </summary>
        /// <param name="new_email">The email to check</param>
        /// <returns>
        ///     True - has a duplicate email
        ///     False - Has either a unique email or no email at all
        /// </returns>
        public  async Task<bool> hasDuplicateEmail<T>(string? new_email)
        where T : class
        {
            if (new_email.IsNullOrEmpty())
            {
                return false;
            }

            // ***Build u => u.email == new_email***

            var parameter = Expression.Parameter(typeof(T), "u"); // Represents parameter u

            // Creates u.email == new_email
            var equality_expression = Expression.Equal(
                Expression.PropertyOrField(parameter, "email"), // builds u.email selector
                Expression.Constant(new_email) // builds an object representing new_email

            );

            // Creates the lambda: u => u.email == new_email
            var email_criteria = Expression.Lambda<Func<T, bool>>(equality_expression, parameter);

            // ***

            return await _context.Set<T>().AnyAsync(email_criteria);
        }

        /// <summary>
        ///  Checks to see if entity model has a valid IP address
        /// </summary>
        /// <param name="ip_address">The IP address to check</param>
        /// <returns>
        ///     True - has a valid IP address
        ///     False - has an invalid IP address
        /// </returns>
        public static Boolean ipChecker(string ip_address)
        {
            return IPAddress.TryParse(ip_address, out _);
        }

        /// <summary>
        ///  Checks to see if entity model has a valid state code
        /// </summary>
        /// <param name="stateCode">The state code to check</param>
        /// <returns>
        ///     True - has a valid state code
        ///     False - has an invalid state code
        /// </returns>
        public static Boolean checkStateCode(string state_code)
        {
            Boolean string_matches = false;

            // 50 state codes, District of Columbia, and five US territories
            string[] state_code_library = {
                "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
                "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
                "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
                "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
                "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY",
                "DC", "AS", "GU", "MP", "PR", "VI"
                };
            for (int i = 0; i < state_code_library.Length; ++i)
                if (state_code == state_code_library[i])
                {
                    string_matches = true;
                }
            return string_matches;
        }

        /// <summary>
        ///  Checks to see if entity model only contains whitespace
        /// </summary>
        /// <param name="input">The state code to check</param>
        /// <returns>
        ///     True - has whitespace
        ///     False - has no whitespace
        /// </returns>
        public static Boolean checkWhitespace(string input)
        {
            return Regex.IsMatch(input, @"^\s*$");
        }

        /// <summary>
        ///  Checks to see if user exists in system
        /// </summary>
        /// <param name="user_id">The user ID to check</param>
        /// <returns>
        ///     True - user exists
        ///     False - user does not exist
        /// </returns>
        /// 
        public async Task<bool> checkUser(int user_id)
        {
            return await _context.Users.AnyAsync(u => u.user_id == user_id);
        }

        /// <summary>
        ///  Checks to see if customer exists in system
        /// </summary>
        /// <param name="customer_id">The customer ID to check</param>
        /// <returns>
        ///     True - customer exists
        ///     False - customer does not exist
        /// </returns>
        /// 
        public async Task<bool> checkCustomer(int customer_id)
        {
            return await _context.Customers.AnyAsync(c => c.customer_id == customer_id);
        }

        /// <summary>
        ///  Checks to see if employee exists in system
        /// </summary>
        /// <param name="employee_id">The employee ID to check</param>
        /// <returns>
        ///     True - employee exists
        ///     False - employee does not exist
        /// </returns>
        /// 
        public async Task<bool> checkEmployee(int employee_id)
        {
            return await _context.Employees.AnyAsync(e => e.employee_id == employee_id);
        }

        /// <summary>
        ///  Checks to see if policy exists in system
        /// </summary>
        /// <param name="policy_id">The policy ID to check</param>
        /// <returns>
        ///     True - policy exists
        ///     False - policy does not exist
        /// </returns>
        /// 
        public async Task<bool> checkPolicy(int policy_id)
        {
            return await _context.Policies.AnyAsync(e => e.policy_id == policy_id);
        }

        /// <summary>
        ///  Checks to see if a file has a duplicate file name
        /// </summary>
        /// <param name="file_name">The file name to check</param>
        /// <returns>
        ///     True - has a duplicate file name
        ///     False - Has a unique file name
        /// </returns>
        public async Task<bool> hasDuplicateFileName(string file_name)
        {
            return await _context.UploadedFiles.AnyAsync(f => f.file_name == file_name);
        }

        /// <summary>
        ///  Checks to see if a file has a valid extension
        /// </summary>
        /// <param name="file">The file to check</param>
        /// <returns>
        ///     File extension or error.
        /// </returns>
        public string hasValidFileExtension(string extension, string[] allowedExtensions)
        {
            if (allowedExtensions.Contains(extension))
            {
                if (extension == "docx")
                {
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document/" + extension;
                }
                else if (extension == "doc")
                {
                    return "text/" + extension;
                }
                else if (extension == "pdf")
                {
                    return "application/" + extension;
                }
                else if (extension == "jpg" || extension == "jpeg")
                {
                    return "image/jpeg";
                }
                else if (extension == "png" || extension == "gif")
                {
                    return "image/" + extension;
                }
            }
            return "Invalid file.";
        }

        /// <summary>
        ///  Checks to see if location exists in system
        /// </summary>
        /// <param name="location_id">The location ID to check</param>
        /// <returns>
        ///     True - location exists
        ///     False - location does not exist
        /// </returns>
        /// 
        public async Task<bool> checkLocation(int location_id)
        {
            return await _context.Locations.AnyAsync(l => l.location_id == location_id);
        }
    }
}
