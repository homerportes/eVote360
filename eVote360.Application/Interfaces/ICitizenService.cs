using eVote360.Application.DTOs.Citizen;

namespace eVote360.Application.Interfaces
{
    public interface ICitizenService
    {
        Task<CitizenDTO?> AddAsync(SaveCitizenDTO dto);
        Task<bool> UpdateAsync(SaveCitizenDTO dto);
        Task<CitizenDTO?> GetById(int id);
        Task<List<CitizenDTO>> GetAll();
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
        Task<bool> IsIdentityDocumentTaken(string identityDocument, int? excludeId = null);
    }
}
