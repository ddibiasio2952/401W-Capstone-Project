using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// The data used to represent the Recommendation
    /// </summary>
    public class Recommendation
    {
        /// <summary>
        /// Unique ID for each recommendation
        /// </summary>
        [Key]
        public int? recommendation_id { get; set; }

        /// <summary>
        /// The user ID that is attached to the recommendation
        /// </summary>
        [Column("user_id")]
        public int? user_id { get; set; }

        /// <summary>
        /// Navigation property to the policy that is attached to the recommendation
        /// </summary>
        [ForeignKey("user_id")]
        public User? user { get; set; }

        /// <summary>
        /// The policy ID that is attached to the recommendation
        /// </summary>
        [Column("policy_id")]
        [Required]
        public required int policy_id { get; set; }

        /// <summary>
        /// Navigation property to the policy that is attached to the recommendation
        /// </summary>
        [ForeignKey("policy_id")]
        public Policy? policy { get; set; }

        /// <summary>
        /// The text of the recommendation
        /// </summary>
        [Column("recommendation_text")]
        [Required]
        [MaxLength(500)]
        public required string recommendation_text { get; set; }


        /// <summary>
        /// The creation DateTime that is attached to the recommendation
        /// </summary>
        public DateTime created_at { get; set; } = DateTime.Now;

    }
}