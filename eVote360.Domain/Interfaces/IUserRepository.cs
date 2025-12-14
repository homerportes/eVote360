using eVote360.Domain.Entities;

namespace eVote360.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> LoginAsync(string userName, string password);
    }
}
