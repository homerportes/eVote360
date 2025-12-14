using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;
using eVote360.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Persistence.Repositories
{
    public class CandidacyRepository : GenericRepository<Candidacy>, ICandidacyRepository
    {
        private readonly EVote360AppContext _context;

        public CandidacyRepository(EVote360AppContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<Candidacy?> GetById(int id)
        {
            return await _context.Set<Candidacy>()
                .Include(c => c.Candidate)
                    .ThenInclude(ca => ca.Citizen)
                .Include(c => c.Candidate)
                    .ThenInclude(ca => ca.Party)
                .Include(c => c.ElectivePosition)
                .Include(c => c.PostulatingParty)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public override async Task<List<Candidacy>> GetAllList()
        {
            return await _context.Set<Candidacy>()
                .Include(c => c.Candidate)
                    .ThenInclude(ca => ca.Citizen)
                .Include(c => c.Candidate)
                    .ThenInclude(ca => ca.Party)
                .Include(c => c.ElectivePosition)
                .Include(c => c.PostulatingParty)
                .ToListAsync();
        }
    }
}
