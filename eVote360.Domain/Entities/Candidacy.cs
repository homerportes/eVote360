
namespace eVote360.Domain.Entities
{
    public class Candidacy
    {
        public int Id { get; set; }
        public int CandidateId { get; set; }
        public int ElectivePositionId { get; set; }
        public int PostulatingPartyId { get; set; }
        public bool IsAlliance { get; set; }

        public Candidate Candidate { get; set; } = null!;
        public ElectivePosition ElectivePosition { get; set; } = null!;
        public PoliticalParty PostulatingParty { get; set; } = null!;
        public ICollection<ElectionCandidacy> ElectionCandidacies { get; set; } = new List<ElectionCandidacy>();
    }
}
