using eVote360.Application.ViewModels.Common;

namespace eVote360.Application.ViewModels.User
{
    public class UserViewModel : PersonViewModel<int>
    {
        public required string Username { get; set; }
        public required int Role { get; set; }
        public int? PartyId { get; set; }
        public string? PartyName { get; set; }
        public string? PartyAcronym { get; set; }
    }
}
