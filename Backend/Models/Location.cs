using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// Represents the location of a client
    /// Geographical data will be used in the Map API
    /// </summary>
    [Table("locations")]
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? location_id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("address")]
        public required string address { get; set; }

        [Required]
        [Column("latitude")]
        public required double latitude { get; set; }

        [Required]
        [Column("longitude")]
        public required double longitude { get; set; }

        /// <summary>
        /// The id of the customer that is attached to the location
        /// </summary>
        [Required]
        [Column("customer_id")]
        public required int customer_id { get; set; }

        /// <summary>
        /// Contains the coordinates for the spatial index
        /// </summary>
        [Required]
        [Column("location", TypeName = "point")]
        public required Point location { get; set; }

        /// <summary>
        ///  Navigation property to the client of the location
        /// </summary>
        [ForeignKey("customer_id")]
        public Customer? customer { get; set; } = null!;

        /// <summary>
        /// Boolean for COPE assessment completion for the policy
        /// </summary>
        [Column("cope_eval")]
        [Required]
        public required bool cope_eval { get; set; }

        /// <summary>
        /// The  date of completing on-site assessment for the policy
        /// </summary>
        [Column("site_eval")]
        [Required]
        public required bool site_eval { get; set; }

        /// <summary>
        /// The date of completing COPE assessment for the policy
        /// </summary>
        [Column("cope_date")]
        public DateOnly? cope_date { get; set; }

        /// <summary>
        /// The  date of completing on-site assessment for the policy
        /// </summary>
        [Column("site_date")]
        public DateOnly? site_date { get; set; }
    }
}