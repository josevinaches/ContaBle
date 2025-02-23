using ContaBle.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace ContaBle.Models
{
    public class CreatePersonViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debes ingresar un correo electrónico válido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "El número de teléfono es obligatorio.")]
        [Phone(ErrorMessage = "Debes ingresar un número de teléfono válido.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [Dni(ErrorMessage = "El DNI no es válido.")]
        public string? Dni { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        public string? Nota { get; set; }

        [Required(ErrorMessage = "El número de socio es obligatorio.")]
        public string? NumeroSocio { get; set; }
        public decimal CuotaSocio { get; set; }
        public decimal CuotaFester { get; set; }
    }
}

