using FalveyInsuranceGroup.Backend.Filters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Represents the data required to create policy dto
    /// </summary>
    public class PolicyDto
    {
        /// <summary>
        /// The required unique identifier for a policy
        /// </summary>
        public int? policy_id { get; set; }

        /// <summary>
        /// The account number for the claim
        /// </summary>
        public required string account_number { get; set; }

        /// <summary>
        /// The customer ID that is attached to the policy
        /// </summary>
        public required int customer_id { get; set; }

        /// <summary>
        /// Navigation property to the customer who is attached to policy
        /// </summary>
        public string? customer_name { get; set; }

        /// <summary>
        /// The manager ID that is attached to the policy
        /// </summary>
        public int manager_id { get; set; }

        /// <summary>
        /// Navigation property to the employee who is attached to policy
        /// </summary>
        public string? manager_name { get; set; }

        /// <summary>
        /// The type of policy (Auto, Property, Liability, Commercial, Marine, Other)
        /// </summary>
        public required string policy_type { get; set; }

        /// <summary>
        /// The status of the policy (Active, Pending, Cancelled, Expired)
        /// </summary>
        public required string status { get; set; }

        /// <summary>
        /// The start DateTime that is attached to the policy
        /// </summary>
        public required DateTime start_date { get; set; }

        /// <summary>
        /// The end DateTime that is attached to the policy
        /// </summary>
        public required DateTime end_date { get; set; }

        /// <summary>
        /// Maximum amount of coverage for the policy
        /// Ensures right precision - 13 digits before decimal and 2 after decimal
        /// </summary>
        public required decimal exposure_amount { get; set; }

        /// <summary>
        /// The creation DateTime that is attached to the policy
        /// </summary>
        public required DateTime created_at { get; set; }

        /// <summary>
        /// The location ID that is attached to the policy
        /// </summary>
        public int location_id { get; set; }

        /// <summary>
        /// Navigation property to the address of the location attached to policy
        /// </summary>
        public string? location_address { get; set; }
    }
}
