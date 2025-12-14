namespace eVote360.Application.DTOs.Vote
{
    public class VoteDTO
    {
        public int Id { get; set; }
        public int ElectionId { get; set; }
        public string ElectionName { get; set; } = string.Empty;
        public int CitizenId { get; set; }
        public string CitizenName { get; set; } = string.Empty;
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public int ElectivePositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public string PartyAcronym { get; set; } = string.Empty;
        public DateTime VotedAt { get; set; }
    }
}
