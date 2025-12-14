namespace eVote360.Application.DTOs.Vote
{
    public class SaveVoteDTO
    {
        public int ElectionId { get; set; }
        public int CitizenId { get; set; }
        public int? CandidateId { get; set; }
        public int ElectivePositionId { get; set; }
    }
}
