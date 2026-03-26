using FalveyInsuranceGroup.Backend.Filters;

namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Represents the data required to create announcement dto
    /// </summary>
    public class AnnouncementDto
    {
        /// <summary>
        /// The required unique identifier for an announcement
        /// </summary>
        public int? announcement_id { get; set; }

        /// <summary>
        /// The title of the announcement 
        /// </summary>
        public required string title { get; set; }

        /// <summary>
        /// The body text of the announcement 
        /// </summary>
        public required string body { get; set; }

        /// <summary>
        /// The publish DateTime of the announcement 
        /// </summary>
        public required DateTime publish_at { get; set; }

        /// <summary>
        /// The expiration DateTime of the announcement 
        /// </summary>
        public required DateTime expire_at { get; set; }

        /// <summary>
        /// The creator of the announcement 
        /// </summary>
        public required int created_by { get; set; }

        /// <summary>
        /// The creation DateTime of the announcement 
        /// </summary>
        public required DateTime created_at { get; set; }
    }
}
