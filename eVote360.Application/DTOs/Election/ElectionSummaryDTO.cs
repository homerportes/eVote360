namespace eVote360.Application.DTOs.Election
{
    public class ElectionSummaryDTO
    {
        public int ElectionId { get; set; }
        public string ElectionName { get; set; } = string.Empty;
        public DateOnly ElectionDate { get; set; }
        public int TotalParties { get; set; }
        public int TotalCandidates { get; set; }
        public int TotalVotes { get; set; }
    }
}
