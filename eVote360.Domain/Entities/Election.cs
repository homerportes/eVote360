
using eVote360.Domain.Common.Enums;

namespace eVote360.Domain.Entities
{
    public class Election
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateOnly Date { get; set; }
        public ElectionStatus Status { get; set; }

        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public ICollection<ElectionCandidacy> ElectionCandidacies { get; set; } = new List<ElectionCandidacy>();
    }
}
