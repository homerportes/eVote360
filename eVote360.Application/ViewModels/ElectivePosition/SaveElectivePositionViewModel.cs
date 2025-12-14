using System.ComponentModel.DataAnnotations;

namespace eVote360.Application.ViewModels.ElectivePosition
{
    public class SaveElectivePositionViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "You must enter the name of the Elective Position")]
        [DataType(DataType.Text)]
        public required string Name { get; set; }
        [Required(ErrorMessage = "You must enter the description of the Elective Position")]
        [DataType(DataType.Text)]
        public required string Description { get; set; } = null!;
        [Required(ErrorMessage = "You must enter the if is active")]
        public required bool IsActive { get; set; } = true;
    }
}
