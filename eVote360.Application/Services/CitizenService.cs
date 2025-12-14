using eVote360.Application.DTOs.Citizen;
using eVote360.Application.Interfaces;
using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;

namespace eVote360.Application.Services
{
    public class CitizenService : ICitizenService
    {
        private readonly ICitizenRepository _citizenRepository;

        public CitizenService(ICitizenRepository citizenRepository)
        {
            _citizenRepository = citizenRepository;
        }

        public async Task<CitizenDTO?> AddAsync(SaveCitizenDTO dto)
        {
            var entity = new Citizen
            {
                Id = 0,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IdentityDocument = dto.IdentityDocument,
                IsActive = true
            };

            var created = await _citizenRepository.AddAsync(entity);
            if (created == null) return null;

            return new CitizenDTO
            {
                Id = created.Id,
                FirstName = created.FirstName,
                LastName = created.LastName,
                Email = created.Email,
                IdentityDocument = created.IdentityDocument,
                IsActive = created.IsActive
            };
        }

        public async Task<bool> UpdateAsync(SaveCitizenDTO dto)
        {
            var entity = await _citizenRepository.GetById(dto.Id);
            if (entity == null) return false;

            entity.FirstName = dto.FirstName;
            entity.LastName = dto.LastName;
            entity.Email = dto.Email;
            entity.IdentityDocument = dto.IdentityDocument;

            var result = await _citizenRepository.UpdateAsync(entity.Id, entity);
            return result != null;
        }

        public async Task<List<CitizenDTO>> GetAll()
        {
            var citizens = await _citizenRepository.GetAllList();
            return citizens.Select(c => new CitizenDTO
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                IdentityDocument = c.IdentityDocument,
                IsActive = c.IsActive
            }).ToList();
        }

        public async Task<CitizenDTO?> GetById(int id)
        {
            var c = await _citizenRepository.GetById(id);
            if (c == null) return null;

            return new CitizenDTO
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                IdentityDocument = c.IdentityDocument,
                IsActive = c.IsActive
            };
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var citizen = await _citizenRepository.GetById(id);
            if (citizen == null) return false;

            citizen.IsActive = true;
            var result = await _citizenRepository.UpdateAsync(id, citizen);
            return result != null;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var citizen = await _citizenRepository.GetById(id);
            if (citizen == null) return false;

            citizen.IsActive = false;
            var result = await _citizenRepository.UpdateAsync(id, citizen);
            return result != null;
        }

        public async Task<bool> IsIdentityDocumentTaken(string identityDocument, int? excludeId = null)
        {
            var citizens = await _citizenRepository.GetAllList();
            return citizens.Any(c =>
                c.IdentityDocument.ToLower() == identityDocument.ToLower() &&
                (!excludeId.HasValue || c.Id != excludeId.Value));
        }
    }
}
