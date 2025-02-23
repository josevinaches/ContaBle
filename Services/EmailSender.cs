using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace ContaBle.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;

        public EmailSender(IOptions<EmailConfiguration> emailConfig)
        {
            _emailConfig = emailConfig.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(_emailConfig.FromEmail))

            {
                throw new ArgumentException("La dirección de remitente no puede ser nula o vacía", nameof(_emailConfig.FromEmail));
            }

            using (var client = new SmtpClient(_emailConfig.Host, _emailConfig.Port))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false; // ✅ Asegurar que no usa credenciales predeterminadas
                client.Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailConfig.FromEmail),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }

        }
    }
}