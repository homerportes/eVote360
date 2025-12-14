using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;
using eVote360.Persistence.Contexts;

namespace eVote360.Persistence.Repositories
{
    public class ElectionCandidacyRepository : GenericRepository<ElectionCandidacy>, IElectionCandidacyRepository
    {
        public ElectionCandidacyRepository(EVote360AppContext context) : base(context)
        {
        }
    }
}
