using eVote360.Domain.Common.CommonEntities;
using eVote360.Domain.Common.Enums;

namespace eVote360.Domain.Entities
{
    public class User : Person<int>
    {
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public int? PartyId { get; set; }
        public PoliticalParty? Party { get; set; }
    }
}
