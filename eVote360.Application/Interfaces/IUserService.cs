using eVote360.Application.DTOs.User;

namespace eVote360.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> AddAsync(SaveUserDTO dto);
        Task<UserDTO> UpdateAsync(SaveUserDTO dto);
        Task<UserDTO?> GetById(int id);
        Task<List<UserDTO>> GetAll();
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
        Task<bool> IsUsernameTaken(string username);
        Task<UserDTO?> LoginAsync(LoginDTO dto);
        Task<bool> UpdatePartyAssignmentAsync(int userId, int? partyId);
    }
}
