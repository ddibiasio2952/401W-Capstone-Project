using FalveyInsuranceGroup.Backend.Filters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// Represents an announcement uploaded to system
    /// </summary>
    public class Announcement
    {
        /// <summary>
        /// The required unique identifier for an announcement
        /// </summary>
        [Column("announcement_id")]
        [Key]
        public int? announcement_id { get; set; }

        /// <summary>
        /// The title of the announcement 
        /// </summary>
        [Column("title")]
        [Required]
        public required string title { get; set; }

        /// <summary>
        /// The publish DateTime of the announcement 
        /// </summary>
        [Column("body")]
        public required string body { get; set; }

        /// <summary>
        /// The expiration DateTime of the announcement 
        /// </summary>
        [Column("publish_at")]
        [Required]
        public required DateTime publish_at { get; set; }

        /// <summary>
        /// The expiration DateTime of the announcement 
        /// </summary>
        [Column("expire_at")]
        [Required]
        public required DateTime expire_at { get; set; }

        /// <summary>
        /// The creator of the announcement 
        /// </summary>
        [Column("created_by")]
        [Required]
        public required int created_by { get; set; }

        /// <summary>
        /// The linked ForeignKey for the creator of the announcement 
        /// </summary>
        [ForeignKey("created_by")]
        public User? User { get; set; }  // links to user_id

        /// <summary>
        /// The creation DateTime of the announcement 
        /// </summary>
        [Column("created_at")]
        public required DateTime created_at { get; set; } = DateTime.Now;
    }
}
