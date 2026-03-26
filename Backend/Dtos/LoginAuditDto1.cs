namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Data to represent a LoginAudit entity
    /// </summary>
    public class LoginAuditDto1
    {
        /// <summary>
        /// ID for the login audit
        /// </summary>
        public long? audit_id { get; set; }

        /// <summary>
        /// The user tied to the login audit
        /// </summary>
        public required int user_id { get; set; }

        /// <summary>
        /// The event tied to the login audit
        /// </summary>
        public required string login_event { get; set; }

        /// <summary>
        /// The ip address tied to a login audit
        /// </summary>
        public string? ip_address { get; set; }

        /// <summary>
        /// What agent the user used to login
        /// </summary>
        public string? user_agent { get; set; }

        /// <summary>
        /// What time that the login happened
        /// </summary>
        public DateTime occurred_at { get; set; } = DateTime.Now;




    }




}
