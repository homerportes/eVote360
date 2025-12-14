namespace eVote360.ViewModels.Voting
{
    public class PositionItemViewModel
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public bool HasVoted { get; set; }
    }
}
