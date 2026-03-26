
namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Holds the geographical information of client's request from frontend
    /// </summary>
    public class GeosearchDto
    {
        /// <summary>
        /// Geosearch types: 'proximity' or 'region'
        /// </summary>
        public required string search_type { get; set; }
        public required double latitude { get; set; }
        public required double longitude { get; set; }

        // Region based fields
        public double? min_latitude { get; set; }
        public double? max_latitude { get; set; }
        public double? min_longitude { get; set; }
        public double? max_longitude { get; set; }

    }
}
