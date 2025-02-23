namespace ContaBle.Services
{
    public class EmailConfiguration
    {
        public string? Host { get; set; } // No "SmtpServer"
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FromEmail { get; set; }
    }
}