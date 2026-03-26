using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// Represents a claim uploaded to system
    /// </summary>
    [Table("claims")]
    public class Claim
    {
        /// <summary>
        /// The required unique identifier for a claim
        /// </summary>
        [Key]
        [Column("claim_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? claim_id { get; set; }

        /// <summary>
        /// The policy id that is attached to the claim
        /// </summary>
        [Required]
        [Column("policy_id")]
        public required int policy_id { get; set; }

        /// <summary>
        /// Navigation property to the policy attached to the claim
        /// </summary
        [ForeignKey("policy_id")]
        public Policy claim_policy { get; set; } = null!;

        [Required]
        [Column("claim_number")]
        public required string claim_number { get; set; }

        /// <summary>
        /// The status of claim (Open, Investigating, Pending, Approved, Denied, Closed)
        /// </summary>
        [Required]
        [Column("status")]
        public required string status { get; set; }

        [Column("date_of_loss")]
        public DateTime? date_of_loss { get; set; }

        [Column("date_reported")]
        public DateTime? date_reported { get; set; }

        /// <summary>
        /// Estimated amount of money set aside to cover the expected cost of a claim
        /// Ensures right precision - 13 digits and 2 after decimal
        /// </summary>
        [Required]
        [Column("reserve_amount", TypeName = "decimal(13,2)")]
        public required decimal reserve_amount { get; set; } = 0;

        /// <summary>
        /// Actual amount of money disbursed to claimant
        /// Ensures right precision - 13 digits and 2 after decimal
        /// </summary>
        [Required]
        [Column("paid_amount", TypeName = "decimal(13,2)")]
        public required decimal paid_amount { get; set; } = 0;

        [MaxLength(300)]
        [Column("memo")]
        public string? memo { get; set; }

        /// <summary>
        /// The employee id the claim is attached to
        /// </summary>
        [Column("assigned_to")]
        public int? assigned_to { get; set; }

        /// <summary>
        /// Navigation property to the employee who is attached to claim
        /// </summary>
        [ForeignKey("assigned_to")]
        public Employee? assigned_employee { get; set; }

        [Column("created_by")]
        public int? created_by { get; set; }

        [ForeignKey("created_by")]
        public User? user_uploader { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.Now;
    }

}