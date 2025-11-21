using System.Net;
using System.Net.Mail;

namespace megadeliciasapi.Services
{
    // 👇 ¡AQUÍ ESTABA EL ERROR! Faltaba ": IEmailService"
    public class EmailService : IEmailService 
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        // Asegúrate de que este método coincida EXACTAMENTE con el de la interfaz
        public async Task SendEmailAsync(string emailDestino, string asunto, string mensajeHtml)
        {
            var emailEmisor = _config["EmailSettings:Email"];
            var password = _config["EmailSettings:Password"];
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"]);

            var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailEmisor, password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailEmisor, "Soporte MegaDelicias"),
                Subject = asunto,
                Body = mensajeHtml,
                IsBodyHtml = true
            };

            mailMessage.To.Add(emailDestino);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}