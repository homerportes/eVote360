
using eVote360.Application.Helpers;
using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;
using eVote360.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Persistence.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly EVote360AppContext _context;
        public UserRepository(EVote360AppContext context) : base(context)
        {
            _context = context;
        }
        public async Task<User?> LoginAsync(string userName, string password)
        {
            string passwordEncrypt = PasswordEncryptation.ComputeSha256Hash(password);

            User? user = await _context.Set<User>()
                .Include(u => u.Party)
                .FirstOrDefaultAsync(u => u.Username == userName && u.PasswordHash == passwordEncrypt);
            return user;
        }

        public override async Task<List<User>> GetAllList()
        {
            return await _context.Set<User>()
                .Include(u => u.Party)
                .ToListAsync();
        }

        public override async Task<User?> GetById(int id)
        {
            return await _context.Set<User>()
                .Include(u => u.Party)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
