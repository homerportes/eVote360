using eVote360.Application.DTOs.PoliticalParty;

namespace eVote360.Application.Interfaces
{
    public interface IPoliticalPartyService
    {
        Task<bool> AddAsync(PoliticalPartyDTO dto);
        Task<bool> UpdateAsync(PoliticalPartyDTO dto);
        Task<PoliticalPartyDTO?> GetById(int id);
        Task<List<PoliticalPartyDTO>> GetAll();
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
