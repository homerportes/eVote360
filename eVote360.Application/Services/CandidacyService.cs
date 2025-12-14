using eVote360.Application.DTOs.Candidacy;
using eVote360.Application.Interfaces;
using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;

namespace eVote360.Application.Services
{
    public class CandidacyService : ICandidacyService
    {
        private readonly ICandidacyRepository _candidacyRepository;

        public CandidacyService(ICandidacyRepository candidacyRepository)
        {
            _candidacyRepository = candidacyRepository;
        }

        public async Task<CandidacyDTO?> AddAsync(SaveCandidacyDTO dto)
        {
            var entity = new Candidacy
            {
                Id = 0,
                CandidateId = dto.CandidateId,
                ElectivePositionId = dto.ElectivePositionId,
                PostulatingPartyId = dto.PostulatingPartyId,
                IsAlliance = dto.IsAlliance
            };

            var created = await _candidacyRepository.AddAsync(entity);
            if (created == null) return null;

            // Reload with related entities
            var result = await _candidacyRepository.GetById(created.Id);
            if (result == null) return null;

            return new CandidacyDTO
            {
                Id = result.Id,
                CandidateId = result.CandidateId,
                CandidateFirstName = result.Candidate?.Citizen?.FirstName ?? "",
                CandidateLastName = result.Candidate?.Citizen?.LastName ?? "",
                ElectivePositionId = result.ElectivePositionId,
                ElectivePositionName = result.ElectivePosition?.Name ?? "",
                PostulatingPartyId = result.PostulatingPartyId,
                PostulatingPartyName = result.PostulatingParty?.Name ?? "",
                PostulatingPartyAcronym = result.PostulatingParty?.Acronym ?? "",
                IsAlliance = result.IsAlliance
            };
        }

        public async Task<CandidacyDTO?> GetById(int id)
        {
            var entity = await _candidacyRepository.GetById(id);
            if (entity == null) return null;

            return new CandidacyDTO
            {
                Id = entity.Id,
                CandidateId = entity.CandidateId,
                CandidateFirstName = entity.Candidate?.Citizen?.FirstName ?? "",
                CandidateLastName = entity.Candidate?.Citizen?.LastName ?? "",
                ElectivePositionId = entity.ElectivePositionId,
                ElectivePositionName = entity.ElectivePosition?.Name ?? "",
                PostulatingPartyId = entity.PostulatingPartyId,
                PostulatingPartyName = entity.PostulatingParty?.Name ?? "",
                PostulatingPartyAcronym = entity.PostulatingParty?.Acronym ?? "",
                IsAlliance = entity.IsAlliance
            };
        }

        public async Task<List<CandidacyDTO>> GetAll()
        {
            var entities = await _candidacyRepository.GetAllList();
            return entities.Select(entity => new CandidacyDTO
            {
                Id = entity.Id,
                CandidateId = entity.CandidateId,
                CandidateFirstName = entity.Candidate?.Citizen?.FirstName ?? "",
                CandidateLastName = entity.Candidate?.Citizen?.LastName ?? "",
                ElectivePositionId = entity.ElectivePositionId,
                ElectivePositionName = entity.ElectivePosition?.Name ?? "",
                PostulatingPartyId = entity.PostulatingPartyId,
                PostulatingPartyName = entity.PostulatingParty?.Name ?? "",
                PostulatingPartyAcronym = entity.PostulatingParty?.Acronym ?? "",
                IsAlliance = entity.IsAlliance
            }).ToList();
        }

        public async Task<List<CandidacyDTO>> GetByPartyId(int partyId)
        {
            var entities = await _candidacyRepository.GetAllList();
            var filtered = entities.Where(c => c.PostulatingPartyId == partyId);

            return filtered.Select(entity => new CandidacyDTO
            {
                Id = entity.Id,
                CandidateId = entity.CandidateId,
                CandidateFirstName = entity.Candidate?.Citizen?.FirstName ?? "",
                CandidateLastName = entity.Candidate?.Citizen?.LastName ?? "",
                ElectivePositionId = entity.ElectivePositionId,
                ElectivePositionName = entity.ElectivePosition?.Name ?? "",
                PostulatingPartyId = entity.PostulatingPartyId,
                PostulatingPartyName = entity.PostulatingParty?.Name ?? "",
                PostulatingPartyAcronym = entity.PostulatingParty?.Acronym ?? "",
                IsAlliance = entity.IsAlliance
            }).ToList();
        }

        public async Task<List<CandidacyDTO>> GetByCandidateId(int candidateId)
        {
            var entities = await _candidacyRepository.GetAllList();
            var filtered = entities.Where(c => c.CandidateId == candidateId);

            return filtered.Select(entity => new CandidacyDTO
            {
                Id = entity.Id,
                CandidateId = entity.CandidateId,
                CandidateFirstName = entity.Candidate?.Citizen?.FirstName ?? "",
                CandidateLastName = entity.Candidate?.Citizen?.LastName ?? "",
                ElectivePositionId = entity.ElectivePositionId,
                ElectivePositionName = entity.ElectivePosition?.Name ?? "",
                PostulatingPartyId = entity.PostulatingPartyId,
                PostulatingPartyName = entity.PostulatingParty?.Name ?? "",
                PostulatingPartyAcronym = entity.PostulatingParty?.Acronym ?? "",
                IsAlliance = entity.IsAlliance
            }).ToList();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _candidacyRepository.GetById(id);
            if (entity == null) return false;

            await _candidacyRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> ExistsCandidacyForPartyAndPosition(int partyId, int positionId)
        {
            var entities = await _candidacyRepository.GetAllList();
            return entities.Any(c => c.PostulatingPartyId == partyId && c.ElectivePositionId == positionId);
        }

        public async Task<CandidacyDTO?> GetCandidacyByPartyAndCandidate(int partyId, int candidateId)
        {
            var entities = await _candidacyRepository.GetAllList();
            var entity = entities.FirstOrDefault(c => c.PostulatingPartyId == partyId && c.CandidateId == candidateId);

            if (entity == null) return null;

            return new CandidacyDTO
            {
                Id = entity.Id,
                CandidateId = entity.CandidateId,
                CandidateFirstName = entity.Candidate?.Citizen?.FirstName ?? "",
                CandidateLastName = entity.Candidate?.Citizen?.LastName ?? "",
                ElectivePositionId = entity.ElectivePositionId,
                ElectivePositionName = entity.ElectivePosition?.Name ?? "",
                PostulatingPartyId = entity.PostulatingPartyId,
                PostulatingPartyName = entity.PostulatingParty?.Name ?? "",
                PostulatingPartyAcronym = entity.PostulatingParty?.Acronym ?? "",
                IsAlliance = entity.IsAlliance
            };
        }

        public async Task<int> CountCandidatesAssignedToPositionsByPartyAsync(int partyId)
        {
            var entities = await _candidacyRepository.GetAllList();

            // Count distinct candidates assigned to positions for this party
            return entities
                .Where(c => c.PostulatingPartyId == partyId)
                .Select(c => c.CandidateId)
                .Distinct()
                .Count();
        }
    }
}
