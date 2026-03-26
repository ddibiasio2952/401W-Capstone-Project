using FalveyInsuranceGroup.Backend.Filters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// Represents a session of user access to system
    /// </summary>
    [Table("sessions")]
    public class Session
    {
        /// <summary>
        /// The required globally unique identifier for a session
        /// </summary>
        [Column("session_id")]
        [Key]
        public Guid? session_id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The user ID that is attached to the session
        /// </summary>
        [Column("user_id")]
        [Required]
        public required int user_id { get; set; }

        /// <summary>
        /// Navigation property to the user who is attached to session
        /// </summary>
        [ForeignKey("user_id")]
        public User? session_user { get; set; }  // links to user_id

        /// <summary>
        /// The session hash that is attached to the session
        /// </summary>
        [Column("session_hash")]
        public required string session_hash { get; set; } = string.Empty;

        /// <summary>
        /// The creation DateTime that is attached to the session
        /// </summary>
        [Column("created_at")]
        public required DateTime created_at { get; set; } = DateTime.Now;

        /// <summary>
        /// The expiration DateTime that is attached to the session
        /// </summary>
        [Column("expires_at")]
        public required DateTime expires_at { get; set; }

        /// <summary>
        /// The revocation DateTime that is attached to the session
        /// </summary>
        [Column("revoked_at")]
        public DateTime? revoked_at { get; set; }

        /// <summary>
        /// The IP address that is attached to the session
        /// </summary>
        [Column("ip_address")]
        [Required]
        public string? ip_address { get; set; } = string.Empty;

        /// <summary>
        /// The user agent that is attached to the session
        /// </summary>
        [Column("user_agent")]
        public string? user_agent { get; set; }

        /// <summary>
        /// Tells if the session is open or closed
        /// </summary>
        [Column("is_active")]
        public bool is_active { get; set;} = true;

    }
}
