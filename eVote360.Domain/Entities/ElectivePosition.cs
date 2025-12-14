namespace eVote360.Domain.Entities
{
    public class ElectivePosition
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Candidacy> Candidacies { get; set; } = new List<Candidacy>();
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
