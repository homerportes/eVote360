namespace eVote360.ViewModels.Voting
{
    public class PositionSelectionViewModel
    {
        public string ElectionName { get; set; } = string.Empty;
        public List<PositionItemViewModel> Positions { get; set; } = new();
    }
}
