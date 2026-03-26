using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// Represents a login of a user in the system
    /// </summary>
    [Table("login_audit")]
    public class LoginAudit
    {
        /// <summary>
        /// audit id for the login event
        /// </summary>
        [Key]
        public long audit_id { get; set; } 

        /// <summary>
        /// the user id in the login event
        /// </summary>
        public required int user_id { get; set; }

        /// <summary>
        /// contains the event type for the login
        /// </summary>
        public required string login_event { get; set; }

        /// <summary>
        /// the ip address in the login event
        /// </summary>
        public string? ip_address { get; set; }

        /// <summary>
        /// The browser/device that was used to perform the login event
        /// </summary>
        public string? user_agent { get; set; }

        /// <summary>
        /// what time the login event occurred at
        /// </summary>
        public DateTime occurred_at { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for foreign key relationships
        /// </summary>
        [ForeignKey("user_id")]
        public User? UserId { get; set; }


    }


}