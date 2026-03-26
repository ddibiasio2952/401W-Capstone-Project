using FalveyInsuranceGroup.Backend.Filters;
using FalveyInsuranceGroup.Backend.Models;

namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Represents the data required to create claim dto
    /// </summary>
    public class ClaimDto
    {
        /// <summary>
        /// The required unique identifier for a claim
        /// </summary>
        [RequiredNull(ErrorMessage = "ERROR: Claim ID should not be provided on creation")]
        public int? claim_id { get; set; }

        /// <summary>
        /// The policy id that is attached to the claim
        /// </summary>
        public required int policy_id { get; set; }

        public Policy? claim_policy { get; set; } = null!;

        public required string claim_number { get; set; }

        /// <summary>
        /// The status of claim (Open, Investigating, Pending, Approved, Denied, Closed)
        /// </summary>
        public required string status { get; set; }

        public DateTime? date_of_loss { get; set; }

        public DateTime? date_reported { get; set; }

        /// <summary>
        /// Estimated amount of money set aside to cover the expected cost of a claim
        /// Ensures right precision - 13 digits and 2 after decimal
        /// </summary>
        public required decimal reserve_amount { get; set; } = 0;

        /// <summary>
        /// Actual amount of money disbursed to claimant
        /// Ensures right precision - 13 digits and 2 after decimal
        /// </summary>
        public required decimal paid_amount { get; set; } = 0;

        public string? memo { get; set; }

        public int? assigned_to { get; set; }

        /// <summary>
        /// Navigation property to the employee who is attached to claim
        /// </summary>
        public string? assigned_employee { get; set; }

        public int? created_by { get; set; }

        public DateTime created_at { get; set; }
    }


}


