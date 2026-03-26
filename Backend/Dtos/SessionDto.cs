using FalveyInsuranceGroup.Backend.Models;

namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Represents a session of user access to system
    /// </summary>
    public class SessionDto
    {

        /// <summary>
        /// The session hash that is attached to the session
        /// </summary>
        public required string session_hash { get; set; }

        /// <summary>
        /// The creation DateTime that is attached to the session
        /// </summary>
        public required DateTime created_at { get; set; }

        /// <summary>
        /// The IP address that is attached to the session
        /// </summary>
        public string? ip_address { get; set; } = string.Empty;

        /// <summary>
        /// The user agent that is attached to the session
        /// </summary>
        public string? user_agent { get; set; }
    }
}
