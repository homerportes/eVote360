using System.ComponentModel.DataAnnotations;

namespace eVote360.ViewModels.Voting
{
    public class DocumentInputViewModel
    {
        [Required(ErrorMessage = "El número de cédula es requerido")]
        [RegularExpression(@"^\d{3}-?\d{7}-?\d{1}$", ErrorMessage = "Formato de cédula inválido. Debe ser 000-0000000-0")]
        [Display(Name = "Número de Cédula")]
        public string DocumentNumber { get; set; } = string.Empty;
    }
}
