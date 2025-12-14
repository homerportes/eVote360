using eVote360.Application.DTOs.ElectivePosition;

namespace eVote360.Application.Interfaces
{
    public interface IElectivePositionService
    {
        Task<bool> AddAsync(ElectivePositionDTO dto);
        Task<bool> UpdateAsync(ElectivePositionDTO dto);
        Task<ElectivePositionDTO?> GetById(int id);
        Task<List<ElectivePositionDTO>> GetAll();
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
    }
}
