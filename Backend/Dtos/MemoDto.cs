using FalveyInsuranceGroup.Backend.Filters;
using FalveyInsuranceGroup.Backend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FalveyInsuranceGroup.Backend.Dtos
{
    public class MemoDto
    {

        /// <summary>
        /// Unique ID for each memo
        /// </summary>
        public int? memo_id { get; set; }

        /// <summary>
        /// The user ID that is attached to the memo
        /// </summary>
        public int? user_id { get; set; }

        /// <summary>
        /// The policy ID that is attached to the memo
        /// </summary>
        public required int policy_id { get; set; }

        /// <summary>
        /// The text of the memo
        /// </summary>
        public required string memo_text { get; set; }


        /// <summary>
        /// The creation DateTime that is attached to the memo
        /// </summary>
        public DateTime created_at { get; set; }

    }
}
