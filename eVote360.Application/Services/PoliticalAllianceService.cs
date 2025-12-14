using eVote360.Application.DTOs.PoliticalAlliance;
using eVote360.Application.Interfaces;
using eVote360.Domain.Common.Enums;
using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;

namespace eVote360.Application.Services
{
    public class PoliticalAllianceService : IPoliticalAllianceService
    {
        private readonly IPoliticalAllianceRepository _allianceRepository;
        private readonly IPoliticalPartyRepository _partyRepository;

        public PoliticalAllianceService(
            IPoliticalAllianceRepository allianceRepository,
            IPoliticalPartyRepository partyRepository)
        {
            _allianceRepository = allianceRepository;
            _partyRepository = partyRepository;
        }

        public async Task<PoliticalAllianceDTO?> CreateAllianceRequestAsync(int requestingPartyId, int receivingPartyId)
        {
            // Validate parties exist
            var requestingParty = await _partyRepository.GetById(requestingPartyId);
            var receivingParty = await _partyRepository.GetById(receivingPartyId);

            if (requestingParty == null || receivingParty == null)
            {
                return null;
            }

            // Check if there's already an active alliance or pending request
            var hasActiveAlliance = await HasActiveAllianceAsync(requestingPartyId, receivingPartyId);
            if (hasActiveAlliance)
            {
                return null;
            }

            // Create alliance request
            var alliance = new PoliticalAlliance
            {
                RequestingPartyId = requestingPartyId,
                ReceivingPartyId = receivingPartyId,
                Status = AllianceStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            var created = await _allianceRepository.AddAsync(alliance);
            if (created == null) return null;

            return await MapToDTO(created);
        }

        public async Task<List<PoliticalAllianceDTO>> GetSentRequestsByPartyAsync(int partyId)
        {
            var alliances = await _allianceRepository.GetAllList();
            var sentRequests = alliances
                .Where(a => a.RequestingPartyId == partyId)
                .OrderByDescending(a => a.RequestedAt)
                .ToList();

            var dtos = new List<PoliticalAllianceDTO>();
            foreach (var alliance in sentRequests)
            {
                var dto = await MapToDTO(alliance);
                if (dto != null) dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<List<PoliticalAllianceDTO>> GetReceivedRequestsByPartyAsync(int partyId)
        {
            var alliances = await _allianceRepository.GetAllList();
            var receivedRequests = alliances
                .Where(a => a.ReceivingPartyId == partyId)
                .OrderByDescending(a => a.RequestedAt)
                .ToList();

            var dtos = new List<PoliticalAllianceDTO>();
            foreach (var alliance in receivedRequests)
            {
                var dto = await MapToDTO(alliance);
                if (dto != null) dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<bool> AcceptAllianceAsync(int allianceId, int partyId)
        {
            var alliance = await _allianceRepository.GetById(allianceId);
            if (alliance == null) return false;

            // Only the receiving party can accept
            if (alliance.ReceivingPartyId != partyId) return false;

            // Only pending alliances can be accepted
            if (alliance.Status != AllianceStatus.Pending) return false;

            alliance.Status = AllianceStatus.Accepted;
            alliance.RespondedAt = DateTime.UtcNow;

            var updated = await _allianceRepository.UpdateAsync(allianceId, alliance);
            return updated != null;
        }

        public async Task<bool> RejectAllianceAsync(int allianceId, int partyId)
        {
            var alliance = await _allianceRepository.GetById(allianceId);
            if (alliance == null) return false;

            // Only the receiving party can reject
            if (alliance.ReceivingPartyId != partyId) return false;

            // Only pending alliances can be rejected
            if (alliance.Status != AllianceStatus.Pending) return false;

            alliance.Status = AllianceStatus.Rejected;
            alliance.RespondedAt = DateTime.UtcNow;

            var updated = await _allianceRepository.UpdateAsync(allianceId, alliance);
            return updated != null;
        }

        public async Task<bool> HasActiveAllianceAsync(int party1Id, int party2Id)
        {
            var alliances = await _allianceRepository.GetAllList();

            // Check for any pending or accepted alliance between the two parties
            return alliances.Any(a =>
                ((a.RequestingPartyId == party1Id && a.ReceivingPartyId == party2Id) ||
                 (a.RequestingPartyId == party2Id && a.ReceivingPartyId == party1Id)) &&
                (a.Status == AllianceStatus.Pending || a.Status == AllianceStatus.Accepted));
        }

        public async Task<List<int>> GetAlliedPartiesAsync(int partyId)
        {
            var alliances = await _allianceRepository.GetAllList();

            var alliedParties = alliances
                .Where(a => a.Status == AllianceStatus.Accepted &&
                           (a.RequestingPartyId == partyId || a.ReceivingPartyId == partyId))
                .Select(a => a.RequestingPartyId == partyId ? a.ReceivingPartyId : a.RequestingPartyId)
                .ToList();

            return alliedParties;
        }

        public async Task<int> CountAcceptedAlliancesByPartyAsync(int partyId)
        {
            var alliances = await _allianceRepository.GetAllList();

            return alliances.Count(a =>
                a.Status == AllianceStatus.Accepted &&
                (a.RequestingPartyId == partyId || a.ReceivingPartyId == partyId));
        }

        public async Task<int> CountPendingRequestsByPartyAsync(int partyId)
        {
            var alliances = await _allianceRepository.GetAllList();

            // Count only received requests that are pending
            return alliances.Count(a =>
                a.ReceivingPartyId == partyId &&
                a.Status == AllianceStatus.Pending);
        }

        private async Task<PoliticalAllianceDTO?> MapToDTO(PoliticalAlliance alliance)
        {
            var requestingParty = await _partyRepository.GetById(alliance.RequestingPartyId);
            var receivingParty = await _partyRepository.GetById(alliance.ReceivingPartyId);

            if (requestingParty == null || receivingParty == null)
            {
                return null;
            }

            return new PoliticalAllianceDTO
            {
                Id = alliance.Id,
                RequestingPartyId = alliance.RequestingPartyId,
                RequestingPartyName = requestingParty.Name,
                RequestingPartyAcronym = requestingParty.Acronym,
                ReceivingPartyId = alliance.ReceivingPartyId,
                ReceivingPartyName = receivingParty.Name,
                ReceivingPartyAcronym = receivingParty.Acronym,
                Status = alliance.Status,
                RequestedAt = alliance.RequestedAt,
                RespondedAt = alliance.RespondedAt
            };
        }
    }
}
