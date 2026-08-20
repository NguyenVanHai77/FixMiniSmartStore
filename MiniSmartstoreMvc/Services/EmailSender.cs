using System.Net;
using System.Net.Mail;

namespace MiniSmartstoreMvc.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ===== LƯU Ý: GỬI EMAIL QUA GMAIL SMTP =====
        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            var smtpServer =
                _configuration["EmailSettings:SmtpServer"];

            var smtpPort =
                int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");

            var senderEmail =
                _configuration["EmailSettings:SenderEmail"];

            var appPassword =
                _configuration["EmailSettings:AppPassword"];

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(
                    senderEmail,
                    appPassword),

                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(
                    senderEmail!,
                    "MiniSmartStore"),

                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
        // ===== KẾT THÚC GỬI EMAIL QUA GMAIL SMTP =====
    }
}