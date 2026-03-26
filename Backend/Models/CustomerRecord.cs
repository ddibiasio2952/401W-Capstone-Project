using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FalveyInsuranceGroup.Backend.Filters;


namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// Represents any type of document to be uploaded
    /// </summary>
    [Table("customer_records")]
    public class CustomerRecord
    {
        /// <summary>
        /// The required unique identifier for the document
        /// </summary>
        [Key]
        [Column("document_id")]
        [RequiredNull(ErrorMessage = "ERROR: Document ID should not be provided on creation")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? document_id { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("file_name")]
        public required string file_name { get; set; }

        [Required]
        [MaxLength(500)]
        [Column("url")]
        public required string url { get; set; }

        [Column("uploaded_by")]
        public int? uploaded_by { get; set; }

        /// <summary>
        /// Navigation property to the employee who uploaded the record
        /// </summary>
        [ForeignKey("uploaded_by")]
        public Employee? employee_uploader { get; set; }

        [Column("uploaded_at")]
        public DateTime uploaded_at { get; set; } = DateTime.Now;

        /// <summary>
        /// The type of entity the record is attached to
        /// </summary>
        [Required]
        [Column("attached_to_type")]
        public required string attached_to_type { get; set; }

        /// <summary>
        /// The id of the entity the record is attached to
        /// </summary>
        [Required]
        [Column("attached_to_id")]
        public required int attached_to_id { get; set; }

        [MaxLength(300)]
        [Column("description")]
        public string? description { get; set; }
    }
}
