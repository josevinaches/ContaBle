using ContaBle.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ContaBle.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        [Required]
        [Dni(ErrorMessage = "El DNI no es válido.")]
        [MaxLength(50)]
        public string? Dni { get; set; }
        [Required]
        public DateTime FechaNacimiento { get; set; }
        public string? Nota { get; set; }
        [Required]
        [StringLength(50)]
        public string? NumeroSocio { get; set; }
        // Propiedad de navegación para el usuario
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Nuevos campos para los pagos
        public decimal CuotaSocio { get; set; }
        public decimal CuotaFester { get; set; }
    }
}



