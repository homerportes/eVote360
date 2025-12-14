using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;
using eVote360.Persistence.Contexts;

namespace eVote360.Persistence.Repositories
{
    public class CitizenRepository : GenericRepository<Citizen>, ICitizenRepository
    {
        public CitizenRepository(EVote360AppContext context) : base(context)
        {
        }
    }
}
