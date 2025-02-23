using System.ComponentModel.DataAnnotations;

namespace ContaBle.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debes ingresar un correo electrónico válido.")]
        public string? Email { get; set; }
    }
}
