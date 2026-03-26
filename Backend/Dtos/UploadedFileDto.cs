using FalveyInsuranceGroup.Backend.Filters;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Represents the data of a file model 
    /// </summary>
    public class UploadedFileDto
    {

        /// <summary>
        /// Unique ID for each file
        /// </summary>
        public int? file_id { get; set; }

        /// <summary>
        /// The name for each file
        /// </summary>
        public required string file_name { get; set; }

        /// <summary>
        /// The media type of the file
        /// </summary>
        public string? media_type { get; set; }

        /// <summary>
        /// The user ID that is attached to the file
        /// </summary>
        public int? user_id { get; set; }

        /// <summary>
        /// The customer ID that is attached to the file
        /// </summary>
        public required int customer_id { get; set; }

        /// <summary>
        /// The policy ID that is attached to the file
        /// </summary>
        public required int policy_id { get; set; }

        /// <summary>
        /// The creation DateTime that is attached to the file
        /// </summary>
        public DateTime created_at { get; set; }

    }
}
