using eVote360.Application.DTOs.Election;

namespace eVote360.Application.Interfaces
{
    public interface IElectionService
    {
        Task<ElectionDTO?> CreateElectionAsync(SaveElectionDTO dto);
        Task<List<ElectionDTO>> GetAllElectionsAsync();
        Task<ElectionDTO?> GetElectionByIdAsync(int electionId);
        Task<ElectionDTO?> GetActiveElectionAsync();
        Task<bool> HasActiveElectionAsync();
        Task<bool> FinishElectionAsync(int electionId);
        Task<ElectionResultDTO?> GetElectionResultsAsync(int electionId);
        Task<ElectionValidationResultDTO> ValidateElectionCreationAsync();
        Task<List<int>> GetAvailableElectionYearsAsync();
        Task<List<ElectionSummaryDTO>> GetElectionSummariesByYearAsync(int year);
    }
}
