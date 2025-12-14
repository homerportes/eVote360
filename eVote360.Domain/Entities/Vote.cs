
namespace eVote360.Domain.Entities
{
    public class Vote
    {
        public int Id { get; set; }
        public int CitizenId { get; set; }
        public int ElectionId { get; set; }
        public int ElectivePositionId { get; set; }
        public int? CandidateId { get; set; }
        public DateTime VoteDate { get; set; } = DateTime.UtcNow;

        public Citizen Citizen { get; set; } = null!;
        public Election Election { get; set; } = null!;
        public ElectivePosition ElectivePosition { get; set; } = null!;
        public Candidate? Candidate { get; set; }
    }
}
