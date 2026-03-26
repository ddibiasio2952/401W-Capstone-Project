using FalveyInsuranceGroup.Backend.Filters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// Represents a customer uploaded to system
    /// </summary>
    [Table("customers")]
    public class Customer
    {
        /// <summary>
        /// The required unique identifier for a customer
        /// </summary>
        [Column("customer_id")]
        [Key]
        public int? customer_id { get; set; }

        /// <summary>
        /// The name that is attached to the customer
        /// </summary>
        [Column("name")]
        [Required]
        [MaxLength(100)]
        public required string name { get; set; }

        /// <summary>
        /// The email that is attached to the customer
        /// </summary>
        [Column("email")]
        [Required]
        [MaxLength(120)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public required string email { get; set; }

        /// <summary>
        /// The phone number that is attached to the customer
        /// </summary>
        [Column("phone")]
        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Please enter a valid phone number with ten digits.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please enter a valid ten-digit phone number (numbers only).")]
        public required string phone { get; set; }

        /// <summary>
        /// The first address line that is attached to the customer
        /// </summary>
        [Column("addr_line1")]
        [Required]
        [StringLength(120, MinimumLength = 4, ErrorMessage = "Please enter a valid address.")]
        public required string addr_line1 { get; set; }

        /// <summary>
        /// The optional second address line that is attached to the customer
        /// </summary>
        [Column("addr_line2")]
        [MaxLength(120)]
        public string? addr_line2 { get; set; }

        /// <summary>
        /// The city that is attached to the customer
        /// </summary>
        [Column("city")]
        [Required]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Please enter a valid city name.")]
        public required string city { get; set; }

        /// <summary>
        /// The state code that is attached to the customer
        /// Full list in helper class.
        /// </summary>
        [Column("state_code")]
        [Required]
        [MaxLength(10)]
        public required string state_code { get; set; }

        /// <summary>
        /// The zip code that is attached to the customer
        /// Ensures 5 or 9-digit zip code.
        /// </summary>
        [Column("zip_code")]
        [StringLength(9, MinimumLength = 5, ErrorMessage = "Please enter a valid zip code between five and nine digits.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Please enter a valid zip code (digits only).")]
        public required string zip_code { get; set; }

        /// <summary>
        /// The creation DateTime that is attached to the customer
        /// </summary>
        [Column("created_at")]
        public required DateTime created_at { get; set; } = DateTime.Now;
    }
}
