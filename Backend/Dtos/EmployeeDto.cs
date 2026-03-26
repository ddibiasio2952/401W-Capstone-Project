namespace FalveyInsuranceGroup.Backend.Dtos
{
    /// <summary>
    /// Represents the data required to create employee dto
    /// </summary>
    public class EmployeeDto
    {
        /// <summary>
        /// The unique identifier for employee
        /// </summary>
        public int? employee_id { get; set; }

        public required string name { get; set; } = string.Empty;

        /// <summary>
        /// The title of employee
        /// </summary>
        public required string title { get; set; }

        public string? email { get; set; }

        public string? phone { get; set; }
    }

}