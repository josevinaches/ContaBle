namespace ContaBle.Models
{
    public class PersonDetailsViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Nota { get; set; } = string.Empty;
        public string NumeroSocio { get; set; } = string.Empty;
        public decimal CuotaSocio { get; set; }
        public decimal CuotaFester { get; set; }
        public string TotalAbonado => (CuotaSocio + CuotaFester).ToString("C");
    }
}
