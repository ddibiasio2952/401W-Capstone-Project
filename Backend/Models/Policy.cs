using FalveyInsuranceGroup.Backend.Filters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// Represents the data required to create policy dto
    /// </summary>
    [Table("policies")]
    public class Policy
    {
        /// <summary>
        /// The required unique identifier for a policy
        /// </summary>
        [Column("policy_id")]
        [Key]
        public int? policy_id { get; set; }

        /// <summary>
        /// The account number for the policy
        /// </summary>
        [Column("account_number")]
        [Required]
        [MaxLength(32)]
        public required string account_number { get; set; }

        /// <summary>
        /// The customer ID that is attached to the policy
        /// </summary>
        [Column("customer_id")]
        [Required]
        public required int customer_id { get; set; }

        /// <summary>
        /// Navigation property to the customer who is attached to policy
        /// </summary>
        [ForeignKey("customer_id")]
        public Customer? customer { get; set; }

        /// <summary>
        /// The manager ID that is attached to the policy
        /// </summary>
        [Column("manager_id")]
        public required int manager_id { get; set; }

        /// <summary>
        /// Navigation property to the employee who is attached to claim
        /// </summary>
        [ForeignKey("manager_id")]
        public Employee? manager { get; set; }

        /// <summary>
        /// The type of policy (Auto, Property, Liability, Commercial, Marine, Other)
        /// </summary>
        [Column("policy_type")]
        [Required]
        [MaxLength(20)]
        public required string policy_type { get; set; } = "Other";

        /// <summary>
        /// The status of the policy (Active, Pending, Cancelled, Expired)
        /// </summary>
        [Column("status")]
        [Required]
        [MaxLength(20)]
        public required string status { get; set; } = "Active";

        /// <summary>
        /// The start DateTime that is attached to the policy
        /// </summary>
        [Column("start_date")]
        [Required]
        public required DateTime start_date { get; set; }

        /// <summary>
        /// The end DateTime that is attached to the policy
        /// </summary>
        [Column("end_date")]
        [Required]
        public required DateTime end_date { get; set; }

        /// <summary>
        /// Maximum amount of coverage for the policy
        /// Ensures right precision - 13 digits before decimal and 2 after decimal
        /// </summary>
        [Column("exposure_amount", TypeName = "decimal(13,2)")]
        [Required]
        [Range(999.99D, 9999999999999.99D, ErrorMessage = "Please enter a valid exposure amount (digits and period only).")]
        public required decimal exposure_amount { get; set; }

        /// <summary>
        /// The creation DateTime that is attached to the policy
        /// </summary>
        [Column("created_at")]
        public required DateTime created_at { get; set; } = DateTime.Now;

        /// <summary>
        /// The location ID that is attached to the policy
        /// </summary>
        [Column("location_id")]
        public required int location_id { get; set; }

        /// <summary>
        /// Navigation property to the employee who is attached to claim
        /// </summary>
        [ForeignKey("location_id")]
        public Location? location { get; set; }
    }
}
