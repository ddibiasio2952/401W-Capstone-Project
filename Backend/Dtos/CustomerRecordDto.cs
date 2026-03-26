namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Represents the data required to create a customer record dto
    /// </summary>
    public class CustomerRecordDto
    {
        /// <summary>
        /// The required unique identifier for document
        /// </summary>
        public int? document_id { get; set; }
        public required string file_name { get; set; }
        public required string url { get; set; }

        /// <summary>
        /// States the id of the employee who uploaded the document.
        /// If null, employee is no longer in system
        /// </summary>
        public int? uploaded_by { get; set; }

        /// <summary>
        /// States the name of the employee who uploaded the document.
        /// If null, employee is no longer in system
        /// </summary>
        public string? uploaded_by_name { get; set; }
        public DateTime uploaded_at { get; set; }

        /// <summary>
        /// The type of entity the document is attached to
        /// </summary>
        public required string attached_to_type { get; set; }
        public string? description { get; set; }
    }
}
