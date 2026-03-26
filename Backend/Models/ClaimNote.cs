using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Models
{
    /// <summary>
    /// Represents a claim note uploaded to system
    /// </summary>
    [Table("claim_notes")]
    public class ClaimNote
    {
        [Key]
        public int? note_id { get; set; } // unique ID for a note
        public required int claim_id { get; set; } // unique ID for a claim, cannot contain null
        public int? author_user_id { get; set; } // property is linked to a foreign key
        public required string note_text { get; set; } // cannot contain null
        public DateTime created_at { get; set; } = DateTime.Now;

        // navigation property for foreign key 
        [ForeignKey("claim_id")]
        public Claim? assigned_claim { get; set; }
        [ForeignKey("author_user_id")]
        public User? user_creator { get; set; }



    }





}