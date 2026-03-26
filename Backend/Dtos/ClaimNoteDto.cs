using FalveyInsuranceGroup.Backend.Filters;
namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Represents the data required to create claimnote dto
    /// </summary>
    public class ClaimNoteDto
    {
        /// <summary>
        /// The required unique identifier for a claim note
        /// </summary>
        [RequiredNull(ErrorMessage = "ERROR: Claim Note ID should not be provided on creation")]
        public int? note_id { get; set; }

        /// <summary>
        /// The claim id attached to the note
        /// </summary>
        public required int claim_id { get; set; }

        /// <summary>
        /// The user id attached to the note
        /// </summary>
        public int? author_user_id { get; set; }

        /// <summary>
        /// Description attached to the claim note
        /// </summary>
        public required string note_text { get; set; }

        /// <summary>
        /// The note's creation time
        /// </summary>
        public DateTime created_at { get; set; } = DateTime.Now;

        /// <summary>
        /// Navigation property to the claim attached to the claim note
        /// </summary>
        public string? assigned_claim { get; set; }
    }

}
