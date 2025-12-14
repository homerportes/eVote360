using System.ComponentModel.DataAnnotations;

namespace eVote360.Application.ViewModels.Common
{
    public class CreatePersonViewModel<TKey>
    {
        public required TKey Id { get; set; }

        [Required(ErrorMessage = "You must enter the first name of the user")]
        [DataType(DataType.Text)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "You must enter the last name of the user")]
        [DataType(DataType.Text)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "You must enter the email address")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; } = null!;
        public required bool IsActive { get; set; } = true;
    }
}
