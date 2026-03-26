namespace FalveyInsuranceGroup.Backend.Dtos
/// <summary>
/// Represents a customer uploaded to system
/// </summary>
{
    public class CustomerDto
    {
        /// <summary>
        /// The required unique identifier for a customer
        /// </summary>
        public int? customer_id { get; set; }

        /// <summary>
        /// The name that is attached to the customer
        /// </summary>
        public required string name { get; set; }

        /// <summary>
        /// The email that is attached to the customer
        /// </summary>
        public required string email { get; set; }

        /// <summary>
        /// The phone number that is attached to the customer
        /// </summary>
        public required string phone { get; set; }

        /// <summary>
        /// The first address line that is attached to the customer
        /// </summary>
        public required string addr_line1 { get; set; }

        /// <summary>
        /// The optional second address line that is attached to the customer
        /// </summary>
        public string? addr_line2 { get; set; }

        /// <summary>
        /// The city that is attached to the customer
        /// </summary>
        public required string city { get; set; }

        /// <summary>
        /// The state code that is attached to the customer
        /// </summary>
        public required string state_code { get; set; }

        /// <summary>
        /// The zip code that is attached to the customer
        /// </summary>
        public required string zip_code { get; set; }

        /// <summary>
        /// The creation DateTime that is attached to the customer
        /// </summary>
        public DateTime created_at { get; set; }

    }
}
