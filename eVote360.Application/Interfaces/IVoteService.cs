using eVote360.Application.DTOs.Vote;

namespace eVote360.Application.Interfaces
{
    /// <summary>
    /// Interface for voting operations
    /// Single Responsibility: Manages voting process
    /// </summary>
    public interface IVoteService
    {
        Task<bool> HasCitizenVotedAsync(int citizenId, int electionId);
        Task<bool> RegisterVoteAsync(SaveVoteDTO voteDto);
        Task<List<VoteDTO>> GetVotesByCitizenAsync(int citizenId, int electionId);
        Task<int> CountVotesByElectionAsync(int electionId);
    }
}
