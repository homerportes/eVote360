using eVote360.Application.DTOs.Candidacy;

namespace eVote360.Application.Interfaces
{
    public interface ICandidacyService
    {
        Task<CandidacyDTO?> AddAsync(SaveCandidacyDTO dto);
        Task<CandidacyDTO?> GetById(int id);
        Task<List<CandidacyDTO>> GetAll();
        Task<List<CandidacyDTO>> GetByPartyId(int partyId);
        Task<List<CandidacyDTO>> GetByCandidateId(int candidateId);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsCandidacyForPartyAndPosition(int partyId, int positionId);
        Task<CandidacyDTO?> GetCandidacyByPartyAndCandidate(int partyId, int candidateId);
        Task<int> CountCandidatesAssignedToPositionsByPartyAsync(int partyId);
    }
}
