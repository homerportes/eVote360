namespace eVote360.Application.ViewModels.Candidate
{
    public class ConfirmCandidateActionViewModel
    {
        public int Id { get; set; }
        public string? CitizenFirstName { get; set; }
        public string? CitizenLastName { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }

        public string FullName => $"{CitizenFirstName} {CitizenLastName}";
    }
}
