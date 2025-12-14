using eVote360.Application.ViewModels.Common;
using System.ComponentModel.DataAnnotations;

namespace eVote360.Application.ViewModels.User
{
    public class UpdateUserViewModel : CreatePersonViewModel<int>
    {
        [Required(ErrorMessage = "You must enter the username of user")]
        [DataType(DataType.Text)]
        public required string Username { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Password must match")]
        [DataType(DataType.Password)]
        public required string ConfirmPassword { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "You must enter the valid role of user")]
        public required int Role { get; set; }
        public int? PartyId { get; set; }
    }
}
