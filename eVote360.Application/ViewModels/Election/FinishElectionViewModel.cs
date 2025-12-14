namespace eVote360.Application.ViewModels.Election
{
    public class FinishElectionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
    }
}
