namespace eVote360.Application.DTOs.Election
{
    public class SaveElectionDTO
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
    }
}
