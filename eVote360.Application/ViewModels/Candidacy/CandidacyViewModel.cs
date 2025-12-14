namespace eVote360.Application.ViewModels.Candidacy
{
    public class CandidacyViewModel
    {
        public int Id { get; set; }
        public string CandidateFullName { get; set; } = string.Empty;
        public string ElectivePositionName { get; set; } = string.Empty;
        public bool IsAlliance { get; set; }
    }
}
