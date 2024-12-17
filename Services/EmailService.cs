using bus_transport_mgt_sys.configuration;
using bus_transport_mgt_sys.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace bus_transport_mgt_sys.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port))
                {
                    client.UseDefaultCredentials = false;
                    // Correct the credentials: use Username and Password separately
                    client.Credentials = new System.Net.NetworkCredential(
                        _emailSettings.Username,
                        _emailSettings.Password
                    );
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.FromEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw; // Re-throw to allow calling method to handle or log
            }
        }
    }
}