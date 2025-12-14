namespace eVote360.ViewModels.Voting
{
    public class VoteConfirmationViewModel
    {
        public string ElectionName { get; set; } = string.Empty;
        public string CitizenName { get; set; } = string.Empty;
        public List<VoteDetailViewModel> VoteDetails { get; set; } = new();
    }
}
