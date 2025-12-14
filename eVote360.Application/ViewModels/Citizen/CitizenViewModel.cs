using eVote360.Application.ViewModels.Common;

namespace eVote360.Application.ViewModels.Citizen
{
    public class CitizenViewModel : PersonViewModel<int>
    {
        public required string IdentityDocument { get; set; }
    }
}
