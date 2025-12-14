using eVote360.Application.DTOs.Email;
using eVote360.Application.DTOs.User;
using eVote360.Application.Helpers;
using eVote360.Application.Interfaces;
using eVote360.Domain.Common.Enums;
using eVote360.Domain.Entities;
using eVote360.Domain.Interfaces;

namespace eVote360.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task<UserDTO?> LoginAsync(LoginDTO dto)
        {
            User? user = await _userRepository.LoginAsync(dto.Username, dto.Password);

            if (user == null)
            {
                return null;
            }

            UserDTO userDto = new()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                Role = (int)user.Role,
                IsActive = user.IsActive,
                PartyId = user.PartyId,
                PartyName = user.Party?.Name,
                PartyAcronym = user.Party?.Acronym
            };

            return userDto;
        }

        public async Task<UserDTO?> AddAsync(SaveUserDTO dto)
        {
            var entity = new User
            {
                Id = 0,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Username = dto.Username,
                PasswordHash = PasswordEncryptation.ComputeSha256Hash(dto.PasswordHash),
                Role = (UserRole)dto.Role,
                PartyId = dto.PartyId,
                IsActive = true
            };

            var created = await _userRepository.AddAsync(entity);
            if (created == null) return null;

            var createdWithParty = await _userRepository.GetById(created.Id);

            await _emailService.SendAsync(new EmailRequestDto
            {
                Subject = "Welcome to InvestmentApp",
                To = created.Email,
                HtmlBody = $"<h1>Your account has been created successfully</h1><p>You can now log in using your username and password - {created.Username}.</p>"
            });
                    
            return new UserDTO
            {
                Id = created.Id,
                FirstName = created.FirstName,
                LastName = created.LastName,
                Email = created.Email,
                Username = created.Username,
                Role = (int)created.Role,
                IsActive = created.IsActive,
                PartyId = created.PartyId,
                PartyName = createdWithParty?.Party?.Name,
                PartyAcronym = createdWithParty?.Party?.Acronym
            };
        }

        public async Task<UserDTO> UpdateAsync(SaveUserDTO dto)
        {
            try
            {

                var entityDb = await _userRepository.GetById(dto.Id);
                if (entityDb == null) throw new InvalidOperationException("User not found");

                User entity = new()
                {
                    Id = dto.Id,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Username = dto.Username,
                    PasswordHash = string.IsNullOrWhiteSpace(dto.PasswordHash) ? entityDb.PasswordHash : PasswordEncryptation.ComputeSha256Hash(dto.PasswordHash),
                    Role = (UserRole)dto.Role,
                    PartyId = dto.PartyId,
                    IsActive = dto.IsActive
                };

                User? returnEntity = await _userRepository.UpdateAsync(entity.Id, entity);
                if (returnEntity == null)
                {
                    throw new InvalidOperationException("Failed to update user");
                }

                // Reload with Party included
                var updatedWithParty = await _userRepository.GetById(returnEntity.Id);

                return new UserDTO()
                {
                    Id = returnEntity.Id,
                    FirstName = returnEntity.FirstName,
                    LastName = returnEntity.LastName,
                    Email = returnEntity.Email,
                    Username = returnEntity.Username,
                    Role = (int)returnEntity.Role,
                    IsActive = returnEntity.IsActive,
                    PartyId = returnEntity.PartyId,
                    PartyName = updatedWithParty?.Party?.Name,
                    PartyAcronym = updatedWithParty?.Party?.Acronym
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error updating user", ex);
            }
        }
        public async Task<List<UserDTO>> GetAll()
        {
            var users = await _userRepository.GetAllList();
            return users.Select(u => new UserDTO
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Username = u.Username,
                Role = (int)u.Role,
                IsActive = u.IsActive,
                PartyId = u.PartyId,
                PartyName = u.Party?.Name,
                PartyAcronym = u.Party?.Acronym
            }).ToList();
        }

        public async Task<UserDTO?> GetById(int id)
        {
            var u = await _userRepository.GetById(id);
            if (u == null) return null;

            return new UserDTO
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Username = u.Username,
                Role = (int)u.Role,
                IsActive = u.IsActive,
                PartyId = u.PartyId,
                PartyName = u.Party?.Name,
                PartyAcronym = u.Party?.Acronym
            };
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return false;

            user.IsActive = true;
            var result = await _userRepository.UpdateAsync(id, user);
            return result != null;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null) return false;

            user.IsActive = false;
            var result = await _userRepository.UpdateAsync(id, user);
            return result != null;
        }

        public async Task<bool> IsUsernameTaken(string username)
        {
            var users = await _userRepository.GetAllList();
            return users.Any(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<bool> HasActiveElection()
        {
            await Task.CompletedTask;
            return false;
        }

        public async Task<bool> UpdatePartyAssignmentAsync(int userId, int? partyId)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) return false;

            user.PartyId = partyId;
            var result = await _userRepository.UpdateAsync(userId, user);
            return result != null;
        }

    }
}
