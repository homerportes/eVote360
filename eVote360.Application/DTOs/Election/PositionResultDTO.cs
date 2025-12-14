namespace eVote360.Application.DTOs.Election
{
    public class PositionResultDTO
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public int TotalVotes { get; set; }
        public List<CandidateResultDTO> CandidateResults { get; set; } = new List<CandidateResultDTO>();
    }
}
