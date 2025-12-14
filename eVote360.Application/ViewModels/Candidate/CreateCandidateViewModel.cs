using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace eVote360.Application.ViewModels.Candidate
{
    public class CreateCandidateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "You must select a citizen")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a valid citizen")]
        public int CitizenId { get; set; }

        [Required(ErrorMessage = "You must select a party")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a valid party")]
        public int PartyId { get; set; }

        [Required(ErrorMessage = "You must upload a photo")]
        public IFormFile? Photo { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
