using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace eVote360.ViewModels.Voting
{
    public class OcrValidationViewModel
    {
        public string DocumentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe subir una imagen de su cédula")]
        [Display(Name = "Imagen de Cédula")]
        public IFormFile? DocumentImage { get; set; }
    }
}
