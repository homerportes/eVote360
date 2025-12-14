using eVote360.Application.DTOs.Candidate;
using eVote360.Application.Interfaces;
using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Application.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly ICitizenRepository _citizenRepository;
        private readonly IPoliticalPartyRepository _partyRepository;

        public CandidateService(
            ICandidateRepository candidateRepository,
            ICitizenRepository citizenRepository,
            IPoliticalPartyRepository partyRepository)
        {
            _candidateRepository = candidateRepository;
            _citizenRepository = citizenRepository;
            _partyRepository = partyRepository;
        }

        public async Task<CandidateDTO?> AddAsync(SaveCandidateDTO dto)
        {
            var entity = new Candidate
            {
                Id = 0,
                CitizenId = dto.CitizenId,
                PartyId = dto.PartyId,
                PhotoUrl = dto.PhotoUrl,
                IsActive = true
            };

            var created = await _candidateRepository.AddAsync(entity);
            if (created == null) return null;

            return await MapToDTO(created);
        }

        public async Task<bool> UpdateAsync(SaveCandidateDTO dto)
        {
            var entity = await _candidateRepository.GetById(dto.Id);
            if (entity == null) return false;

            entity.CitizenId = dto.CitizenId;
            entity.PartyId = dto.PartyId;

            // Solo actualizar la foto si se proporcionÃ³ una nueva
            if (!string.IsNullOrEmpty(dto.PhotoUrl))
            {
                entity.PhotoUrl = dto.PhotoUrl;
            }

            var result = await _candidateRepository.UpdateAsync(entity.Id, entity);
            return result != null;
        }

        public async Task<List<CandidateDTO>> GetAll()
        {
            var candidates = await _candidateRepository.GetAllList();
            var dtoList = new List<CandidateDTO>();

            foreach (var candidate in candidates)
            {
                var dto = await MapToDTO(candidate);
                if (dto != null)
                    dtoList.Add(dto);
            }

            return dtoList;
        }

        public async Task<List<CandidateDTO>> GetByPartyId(int partyId)
        {
            var candidates = await _candidateRepository.GetAllList();
            var filtered = candidates.Where(c => c.PartyId == partyId).ToList();
            var dtoList = new List<CandidateDTO>();

            foreach (var candidate in filtered)
            {
                var dto = await MapToDTO(candidate);
                if (dto != null)
                    dtoList.Add(dto);
            }

            return dtoList;
        }

        public async Task<CandidateDTO?> GetById(int id)
        {
            var candidate = await _candidateRepository.GetById(id);
            if (candidate == null) return null;

            return await MapToDTO(candidate);
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var candidate = await _candidateRepository.GetById(id);
            if (candidate == null) return false;

            candidate.IsActive = true;
            var result = await _candidateRepository.UpdateAsync(id, candidate);
            return result != null;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var candidate = await _candidateRepository.GetById(id);
            if (candidate == null) return false;

            candidate.IsActive = false;
            var result = await _candidateRepository.UpdateAsync(id, candidate);
            return result != null;
        }

        public async Task<int> CountActiveCandidatesByPartyAsync(int partyId)
        {
            var candidates = await _candidateRepository.GetAllList();
            return candidates.Count(c => c.PartyId == partyId && c.IsActive);
        }

        public async Task<int> CountInactiveCandidatesByPartyAsync(int partyId)
        {
            var candidates = await _candidateRepository.GetAllList();
            return candidates.Count(c => c.PartyId == partyId && !c.IsActive);
        }

        private async Task<CandidateDTO?> MapToDTO(Candidate candidate)
        {
            var citizen = await _citizenRepository.GetById(candidate.CitizenId);
            var party = await _partyRepository.GetById(candidate.PartyId);

            if (citizen == null || party == null) return null;

            return new CandidateDTO
            {
                Id = candidate.Id,
                CitizenId = candidate.CitizenId,
                PartyId = candidate.PartyId,
                PhotoUrl = candidate.PhotoUrl,
                IsActive = candidate.IsActive,
                CitizenFirstName = citizen.FirstName,
                CitizenLastName = citizen.LastName,
                PartyName = party.Name,
                PartyAcronym = party.Acronym
            };
        }
    }
}
