using FalveyInsuranceGroup.Backend.Filters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Dtos
{
    public class RecommendationDto
    {

        /// <summary>
        /// Unique ID for each recommendation
        /// </summary>
        public int? recommendation_id { get; set; }

        /// <summary>
        /// The user ID that is attached to the recommendation
        /// </summary>
        public int? user_id { get; set; }

        /// <summary>
        /// The policy ID that is attached to the recommendation
        /// </summary>
        public required int policy_id { get; set; }

        /// <summary>
        /// The text of the recommendation
        /// </summary>
        public required string recommendation_text { get; set; }


        /// <summary>
        /// The creation DateTime that is attached to the recommendation
        /// </summary>
        public DateTime created_at { get; set; }

    }
}
