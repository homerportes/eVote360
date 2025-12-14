using eVote360.Application.DTOs.Common;

namespace eVote360.Application.DTOs.User
{
    public class SaveUserDTO : PersonDTO<int>
    {
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required int Role { get; set; }
        public int? PartyId { get; set; }
    }
}
