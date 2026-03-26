using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// Represents the application releases
    /// </summary>
    [Table("releases")]
    public class Release
    {
        /// <summary>
        /// The unique identifier for releases
        /// </summary>
        [Key]
        [Required]
        [MaxLength(10)]
        [Column("version")]
        public required string version { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("summary")]
        public required string summary { get; set; }

        [Column("start_date")]
        public DateOnly start_date { get; set; }

        [Column("rollout_date")]
        public DateOnly? rollout_date { get; set; }

        [Column("complete_date")]
        public DateOnly? complete_date { get; set; }

        [MaxLength(400)]
        [Column("notes")]
        public string? notes { get; set; }

        [MaxLength(400)]
        [Column("hotfix_notes")]
        public string? hotfix_notes { get; set; }

    }

}