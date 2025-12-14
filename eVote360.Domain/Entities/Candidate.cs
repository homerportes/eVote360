
namespace eVote360.Domain.Entities
{
    public class Candidate
    {
        public int Id { get; set; }
        public int CitizenId { get; set; }
        public int PartyId { get; set; }
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public Citizen Citizen { get; set; } = null!;
        public PoliticalParty Party { get; set; } = null!;
        public ICollection<Candidacy> Candidacies { get; set; } = new List<Candidacy>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    }
}
