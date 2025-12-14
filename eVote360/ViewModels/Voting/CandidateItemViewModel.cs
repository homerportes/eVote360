namespace eVote360.ViewModels.Voting
{
    public class CandidateItemViewModel
    {
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string PartyAcronym { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
    }
}
