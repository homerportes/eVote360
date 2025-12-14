using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;
using eVote360.Persistence.Contexts;

namespace eVote360.Persistence.Repositories
{
    public class CandidateRepository : GenericRepository<Candidate>, ICandidateRepository
    {
        public CandidateRepository(EVote360AppContext context) : base(context)
        {
        }
    }
}
