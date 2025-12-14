namespace eVote360.Domain.Entities
{
    public class PoliticalParty
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Acronym { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<User> Leaders { get; set; } = new List<User>();
        public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
        public ICollection<Candidacy> Candidacies { get; set; } = new List<Candidacy>();
        public ICollection<PoliticalAlliance> RequestedAlliances { get; set; } = new List<PoliticalAlliance>();
        public ICollection<PoliticalAlliance> ReceivedAlliances { get; set; } = new List<PoliticalAlliance>();
    }
}
