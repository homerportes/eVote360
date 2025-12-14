using eVote360.Application.DTOs.Candidate;

namespace eVote360.Application.Interfaces
{
    public interface ICandidateService
    {
        Task<CandidateDTO?> AddAsync(SaveCandidateDTO dto);
        Task<bool> UpdateAsync(SaveCandidateDTO dto);
        Task<CandidateDTO?> GetById(int id);
        Task<List<CandidateDTO>> GetAll();
        Task<List<CandidateDTO>> GetByPartyId(int partyId);
        Task<bool> ActivateAsync(int id);
        Task<bool> DeactivateAsync(int id);
        Task<int> CountActiveCandidatesByPartyAsync(int partyId);
        Task<int> CountInactiveCandidatesByPartyAsync(int partyId);
    }
}
