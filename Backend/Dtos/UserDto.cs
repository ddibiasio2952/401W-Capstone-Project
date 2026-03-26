using FalveyInsuranceGroup.Backend.Filters;

namespace FalveyInsuranceGroup.Backend.Dtos
{

    /// <summary>
    /// Represents the data for the User model
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// unique identifier for a user
        /// </summary>
        [RequiredNull(ErrorMessage = "ERROR: User ID should not be provided on creation")]
        public int? user_id { get; set; }

        /// <summary>
        /// email address attached to a user
        /// </summary>
        public required string email { get; set; }

        /// <summary>
        /// hash assigned to a user
        /// </summary>
        public required string password_hash { get; set; }

        /// <summary>
        /// The user's role
        /// </summary>
        public required string role { get; set; }


        /// <summary>
        /// The employee ID for a user
        /// </summary>
        public required int employee_id { get; set; }

        /// <summary>
        /// Value tells if user is active
        /// </summary>
        public required bool is_active { get; set; } = true;

        /// <summary>
        /// Tells what date the user was created on
        /// </summary>
        public required DateTime created_at { get; set; } = DateTime.Now;

        /// <summary>
        /// Tells what time the User was updated on
        /// </summary>
        public DateTime? updated_at { get; set; } = null;


    }






}

