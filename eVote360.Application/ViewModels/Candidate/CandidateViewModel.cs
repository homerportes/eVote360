namespace eVote360.Application.ViewModels.Candidate
{
    public class CandidateViewModel
    {
        public int Id { get; set; }
        public int CitizenId { get; set; }
        public int PartyId { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }

        // Display properties
        public string? CitizenFirstName { get; set; }
        public string? CitizenLastName { get; set; }
        public string? PartyName { get; set; }
        public string? PartyAcronym { get; set; }
        public string? ElectivePositionName { get; set; }

        public string FullName => $"{CitizenFirstName} {CitizenLastName}";
    }
}
