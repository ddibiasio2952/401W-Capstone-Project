using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// The data used to represent the file uploaded by a client
    /// </summary>
    public class UploadedFile
    {
        /// <summary>
        /// Unique ID for each file
        /// </summary>
        [Key]
        public int? file_id { get; set; }

        /// <summary>
        /// The name of the file
        /// </summary>
        [Column("file_name")]
        [Required]
        [MaxLength(255)]
        public required string file_name { get; set; }

        /// <summary>
        /// The data of the file
        /// </summary>
        [Column("file_data")]
        public byte[]? file_data { get; set; }

        /// <summary>
        /// The media type of the file
        /// </summary>
        [Column("media_type")]
        [MaxLength(100)]
        public string? media_type { get; set; }

        /// <summary>
        /// The user ID that is attached to the file
        /// </summary>
        [Column("user_id")]
        public int? user_id { get; set; }

        /// <summary>
        /// Navigation property to the user that is attached to the file
        /// </summary>
        [ForeignKey("user_id")]
        public User? user { get; set; }

        /// <summary>
        /// The customer ID that is attached to the file
        /// </summary>
        [Column("customer_id")]
        public required int customer_id { get; set; }

        /// <summary>
        /// Navigation property to the customer that is attached to the file
        /// </summary>
        [ForeignKey("customer_id")]
        public Customer? customer { get; set; }

        /// <summary>
        /// The policy ID that is attached to the file
        /// </summary>
        [Column("policy_id")]
        [Required]
        public required int policy_id { get; set; }

        /// <summary>
        /// Navigation property to the policy that is attached to the file
        /// </summary>
        [ForeignKey("policy_id")]
        public Policy? policy { get; set; }

        /// <summary>
        /// The creation DateTime that is attached to the file
        /// </summary>
        public DateTime created_at { get; set; } = DateTime.Now;

    }
}