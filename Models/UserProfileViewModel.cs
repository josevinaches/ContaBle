using System;
using System.Collections.Generic;
namespace ContaBle.Models
{
    public class UserProfileViewModel
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Dni { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string? Nota { get; set; }
        public string? NumeroSocio { get; set; }
        public List<Person>? Persons { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public decimal CuotaSocio { get; set; }
        public decimal CuotaFester { get; set; }
    }
}
