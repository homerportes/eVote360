using eVote360.Application.ViewModels.Common;
using System.ComponentModel.DataAnnotations;

namespace eVote360.Application.ViewModels.Citizen
{
    public class CreateCitizenViewModel : CreatePersonViewModel<int>
    {
        [Required(ErrorMessage = "You must enter the identity document")]
        [DataType(DataType.Text)]
        public required string IdentityDocument { get; set; }
    }
}
