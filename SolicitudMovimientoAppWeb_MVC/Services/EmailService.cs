using System.Net;
using System.Net.Mail;

namespace SolicitudMovimientoAppWeb_MVC.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        //Constructor
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //Metodo asincronico para enviar un correo
        public async Task SendEmailAsync(string toEmail, string subject, string body, string attachmentPath)
        {
            //Configura el cliente del email
            var smtpClient = new SmtpClient
            {
                Host = _configuration["EmailSettings:SmtpServer"],
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:Username"],
                    _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            //Creacion del email a enviar
            using (var mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(_configuration["EmailSettings:SenderEmail"], _configuration["EmailSettings:SenderName"]);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                if (!string.IsNullOrEmpty(attachmentPath))
                {
                    mailMessage.Attachments.Add(new Attachment(attachmentPath));
                }

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}
