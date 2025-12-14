namespace eVote360.Application.DTOs.Election
{
    public class ElectionResultDTO
    {
        public int ElectionId { get; set; }
        public string ElectionName { get; set; } = string.Empty;
        public DateOnly ElectionDate { get; set; }
        public List<PositionResultDTO> PositionResults { get; set; } = new List<PositionResultDTO>();
    }
}
