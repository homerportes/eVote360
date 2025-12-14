using eVote360.Application.DTOs.Common;
using eVote360.Domain.Common.Enums;

namespace eVote360.Application.DTOs.User
{
    public class UserDTO : PersonDTO<int>
    {
        public required string Username { get; set; }
        public int Role { get; set; }
        public int? PartyId { get; set; }
        public string? PartyName { get; set; }
        public string? PartyAcronym { get; set; }
    }
}
