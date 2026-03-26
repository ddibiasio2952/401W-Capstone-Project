namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Data to represent a LoginAudit entity
    /// </summary>
    public class LoginAuditDto
    {

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
        /// The time that the logged event occurred at
        /// </summary>
        public required DateTime occurred_at { get; set; }

        




    }




}
