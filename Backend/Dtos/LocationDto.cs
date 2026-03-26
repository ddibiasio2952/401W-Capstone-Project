using FalveyInsuranceGroup.Backend.Filters;

namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Represents the data for the location model
    /// </summary>
    public class LocationDto
    {   
        [RequiredNull(ErrorMessage = "ERROR: Location ID should not be provided on creation")]
        public int? location_id { get; set; }
        public required string address { get; set; }
        public required double latitude { get; set; }
        public required double longitude { get; set; }
        public required int customer_id { get; set; }

        /// <summary>
        /// Boolean for COPE assessment completion for the policy
        /// </summary>
        public required bool cope_eval { get; set; }

        /// <summary>
        /// The  date of completing on-site assessment for the policy
        /// </summary>
        public required bool site_eval { get; set; }

        /// <summary>
        /// The date of completing COPE assessment for the policy
        /// </summary>
        public DateOnly? cope_date { get; set; }

        /// <summary>
        /// The  date of completing on-site assessment for the policy
        /// </summary>
        public DateOnly? site_date { get; set; }
    }
}