using eVote360.Application.DTOs.PoliticalAlliance;

namespace eVote360.Application.Interfaces
{
    public interface IPoliticalAllianceService
    {
        Task<PoliticalAllianceDTO?> CreateAllianceRequestAsync(int requestingPartyId, int receivingPartyId);
        Task<List<PoliticalAllianceDTO>> GetSentRequestsByPartyAsync(int partyId);
        Task<List<PoliticalAllianceDTO>> GetReceivedRequestsByPartyAsync(int partyId);
        Task<bool> AcceptAllianceAsync(int allianceId, int partyId);
        Task<bool> RejectAllianceAsync(int allianceId, int partyId);
        Task<bool> HasActiveAllianceAsync(int party1Id, int party2Id);
        Task<List<int>> GetAlliedPartiesAsync(int partyId);
        Task<int> CountAcceptedAlliancesByPartyAsync(int partyId);
        Task<int> CountPendingRequestsByPartyAsync(int partyId);
    }
}
