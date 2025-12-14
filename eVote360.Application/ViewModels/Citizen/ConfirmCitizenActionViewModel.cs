using eVote360.Application.ViewModels.Common;

namespace eVote360.Application.ViewModels.Citizen
{
    public class ConfirmCitizenActionViewModel : ConfirmPersonActionViewModel<int>
    {
        public required string IdentityDocument { get; set; }
    }
}
