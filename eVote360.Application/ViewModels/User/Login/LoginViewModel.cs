using System.ComponentModel.DataAnnotations;

namespace eVote360.Application.ViewModels.User.Login
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "You must enter the usernme")]
        [DataType(DataType.Text)]
        public required string Username { get; set; }
        [Required(ErrorMessage = "You must enter the password of user")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
