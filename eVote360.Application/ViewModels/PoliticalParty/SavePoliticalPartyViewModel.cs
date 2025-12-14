using System.ComponentModel.DataAnnotations;

namespace eVote360.Application.ViewModels.PoliticalParty
{
    public class SavePoliticalPartyViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "You must enter the party name")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "You must enter the acronym")]
        [MaxLength(10, ErrorMessage = "The acronym must be less than 10 characters")]
        public required string Acronym { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public required bool IsActive { get; set; } = true;
    }
}
