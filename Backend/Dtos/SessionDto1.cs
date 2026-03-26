using FalveyInsuranceGroup.Backend.Models;

namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Represents a session of user access to system
    /// </summary>
    public class SessionDto1
    {
        /// <summary>
        /// The required globally unique identifier for a session
        /// </summary>
        public Guid? session_id { get; set; }

        /// <summary>
        /// The user ID that is attached to the session
        /// </summary>
        public required int user_id { get; set; }

        /// <summary>
        /// The session hash that is attached to the session
        /// </summary>
        public required string session_hash { get; set; }

        /// <summary>
        /// The creation DateTime that is attached to the session
        /// </summary>
        public required DateTime created_at { get; set; }

        /// <summary>
        /// The expiration DateTime that is attached to the session
        /// </summary>
        public DateTime? expires_at { get; set; }

        /// <summary>
        /// The revocation DateTime that is attached to the session
        /// </summary>
        public DateTime? revoked_at { get; set; }

        /// <summary>
        /// The IP address that is attached to the session
        /// </summary>
        public string? ip_address { get; set; } = string.Empty;

        /// <summary>
        /// The user agent that is attached to the session
        /// </summary>
        public string? user_agent { get; set; }

        /// <summary>
        /// Tells if a session is open or closed
        /// </summary>
        public bool is_active { get; set;} = true;
    }
}
