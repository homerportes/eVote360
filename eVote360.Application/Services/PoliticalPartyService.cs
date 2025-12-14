using eVote360.Application.DTOs.PoliticalParty;
using eVote360.Application.Interfaces;
using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Application.Services
{
    public class PoliticalPartyService : IPoliticalPartyService
    {
        private readonly IPoliticalPartyRepository _repository;
        private readonly ICandidateRepository _candidateRepository;

        public PoliticalPartyService(
            IPoliticalPartyRepository repository,
            ICandidateRepository candidateRepository)
        {
            _repository = repository;
            _candidateRepository = candidateRepository;
        }

        public async Task<bool> AddAsync(PoliticalPartyDTO dto)
        {
            try
            {
                var entity = new PoliticalParty
                {
                    Id = 0,
                    Name = dto.Name,
                    Acronym = dto.Acronym,
                    Description = dto.Description,
                    LogoUrl = dto.LogoUrl,
                    IsActive = dto.IsActive
                };

                var saved = await _repository.AddAsync(entity);
                return saved != null;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> UpdateAsync(PoliticalPartyDTO dto)
        {
            try
            {
                var entity = new PoliticalParty
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Acronym = dto.Acronym,
                    Description = dto.Description,
                    LogoUrl = dto.LogoUrl,
                    IsActive = dto.IsActive
                };

                var updated = await _repository.UpdateAsync(dto.Id, entity);
                return updated != null;
            }
            catch
            {
                return false;
            }
        }
        public async Task<PoliticalPartyDTO?> GetById(int id)
        {
            try
            {
                var entity = await _repository.GetAllQuery()
                                              .FirstOrDefaultAsync(p => p.Id == id);
                if (entity == null) return null;

                return new PoliticalPartyDTO
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Acronym = entity.Acronym,
                    Description = entity.Description,
                    LogoUrl = entity.LogoUrl,
                    IsActive = entity.IsActive
                };
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<PoliticalPartyDTO>> GetAll()
        {
            try
            {
                var entities = await _repository.GetAllList();
                return entities.Select(p => new PoliticalPartyDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Acronym = p.Acronym,
                    Description = p.Description,
                    LogoUrl = p.LogoUrl,
                    IsActive = p.IsActive
                }).ToList();
            }
            catch
            {
                return new List<PoliticalPartyDTO>();
            }
        }
        public async Task<bool> ActivateAsync(int id)
        {
            var entity = await _repository.GetById(id);
            if (entity == null) return false;

            entity.IsActive = true;
            var result = await _repository.UpdateAsync(id, entity);
            return result != null;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var entity = await _repository.GetById(id);
            if (entity == null) return false;

            entity.IsActive = false;
            var result = await _repository.UpdateAsync(id, entity);
            if (result == null) return false;

            // Desactivar en cascada todos los candidatos registrados bajo este partido
            var allCandidates = await _candidateRepository.GetAllList();
            var candidatesForParty = allCandidates
                .Where(c => c.PartyId == id)
                .ToList();

            foreach (var candidate in candidatesForParty)
            {
                candidate.IsActive = false;
                await _candidateRepository.UpdateAsync(candidate.Id, candidate);
            }

            return true;
        }

    }
}
