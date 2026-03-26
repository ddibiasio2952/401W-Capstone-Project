using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// The data used to represent the User
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique ID for each user
        /// </summary>
        [Key]
        public int user_id { get; set; } 

        /// <summary>
        /// email address attached to a user
        /// </summary>
        public required string email { get; set; }

        /// <summary>
        /// user's unique hash for password
        /// </summary>
        public required string password_hash { get; set; }

        /// <summary>
        /// role assigned to user
        /// </summary>
        public required string role { get; set; }


        /// <summary>
        /// has an id if the user is an employee
        /// </summary>
        public required int employee_id { get; set; }


        /// <summary>
        /// Value tells if user is active
        /// </summary>
        public bool is_active { get; set; } = true;

        /// <summary>
        /// the data that the user was created on
        /// </summary>
        public DateTime created_at { get; set; } = DateTime.UtcNow;


        /// <summary>
        /// holds the date that the user was updated on
        /// </summary>
        public DateTime? updated_at { get; set; } = null;



        /// <summary>
        /// Navigation property to hold a Employee entity
        /// </summary>
        [ForeignKey("employee_id")]
        public Employee? EmployeeId { get; set; }





    }



    
}