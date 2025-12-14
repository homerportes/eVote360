using eVote360.Domain.Common.CommonEntities;

namespace eVote360.Domain.Entities
{
    public class Citizen : Person<int>
    {
        public required string IdentityDocument { get; set; }
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
