namespace FalveyInsuranceGroup.Backend.Dtos
{
    public class ClaimSummaryDto
    {
        public int ClaimId { get; set; }
        public string ClaimNumber { get; set; }
        public string InsuredName { get; set; }
        public string Status { get; set; }
        public DateTime? DateReported { get; set; }
    }
}
