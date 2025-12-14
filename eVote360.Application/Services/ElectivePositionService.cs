using eVote360.Application.DTOs.ElectivePosition;
using eVote360.Application.Interfaces;
using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eVote360.Application.Services
{
    public class ElectivePositionService : IElectivePositionService
    {
        private readonly IElectivePositionRepository _repository;
        private readonly ICandidacyRepository _candidacyRepository;
        private readonly ICandidateRepository _candidateRepository;

        public ElectivePositionService(
            IElectivePositionRepository repository,
            ICandidacyRepository candidacyRepository,
            ICandidateRepository candidateRepository)
        {
            _repository = repository;
            _candidacyRepository = candidacyRepository;
            _candidateRepository = candidateRepository;
        }
        public async Task<bool> AddAsync(ElectivePositionDTO dto)
        {
            try
            {
                ElectivePosition entity = new()
                {
                    Id = 0,
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = dto.IsActive
                };

                ElectivePosition? result = await _repository.AddAsync(entity);
                if (result == null) return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ElectivePositionDTO dto)
        {
            try
            {
                ElectivePosition entity = new()
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = dto.IsActive
                };

                ElectivePosition? result = await _repository.UpdateAsync(entity.Id, entity);
                if (result == null) return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ElectivePositionDTO?> GetById(int id)
        {
            try
            {
                var query = _repository.GetAllQuery();
                var entity = await query.FirstOrDefaultAsync(e => e.Id == id);

                if (entity == null) return null;

                ElectivePositionDTO dto = new()
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Description = entity.Description,
                    IsActive = entity.IsActive
                };

                return dto;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<ElectivePositionDTO>> GetAll()
        {
            try
            {
                var entities = await _repository.GetAllList();

                var list = entities.Select(s => new ElectivePositionDTO()
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    IsActive = s.IsActive
                }).ToList();

                return list;
            }
            catch (Exception)
            {
                return [];
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

            // Desactivar en cascada todos los candidatos asociados a este puesto
            var allCandidacies = await _candidacyRepository.GetAllList();
            var candidaciesForPosition = allCandidacies
                .Where(c => c.ElectivePositionId == id)
                .ToList();

            // Obtener los IDs unicos de candidatos asociados
            var candidateIds = candidaciesForPosition
                .Select(c => c.CandidateId)
                .Distinct()
                .ToList();

            // Desactivar cada candidato
            foreach (var candidateId in candidateIds)
            {
                var candidate = await _candidateRepository.GetById(candidateId);
                if (candidate != null && candidate.IsActive)
                {
                    candidate.IsActive = false;
                    await _candidateRepository.UpdateAsync(candidateId, candidate);
                }
            }

            return true;
        }

    }
}
