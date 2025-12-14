namespace eVote360.ViewModels.Voting
{
    public class CandidateSelectionViewModel
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public List<CandidateItemViewModel> Candidates { get; set; } = new();
    }
}
