using System.ComponentModel.DataAnnotations;

namespace ContaBle.Models
{
    public class RegisterViewModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Nombre de usuario")]
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Debes seleccionar una compañía.")]
        public int CompanyId { get; set; }

    }
}