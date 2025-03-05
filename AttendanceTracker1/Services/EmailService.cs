using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AttendanceTracker1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string sender_email, string name, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(emailSettings["SenderName"], emailSettings["SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;

            string formattedBody = $@"
                <div style='max-width: 600px; margin: auto; font-family: Arial, sans-serif; color: #333; background-color: #f9f9f9; padding: 20px; border-radius: 8px; box-shadow: 0px 2px 10px rgba(0, 0, 0, 0.1);'>
                    <div style='background-color: #007bff; color: #ffffff; text-align: center; padding: 15px; border-radius: 8px 8px 0 0;'>
                        <h2 style='margin: 0; font-size: 22px;'>New Email Received</h2>
                    </div>
                    <div style='padding: 20px; background-color: #ffffff; border-radius: 0 0 8px 8px;'>
                        <p style='font-size: 16px; margin: 10px 0;'><strong>Sender Name:</strong> {name}</p>
                        <p style='font-size: 16px; margin: 10px 0;'><strong>Sender Email:</strong> {sender_email}</p>
                        <hr style='border: 1px solid #ddd; margin: 20px 0;'>
                        <p style='font-size: 14px; color: #555; line-height: 1.6;'>{body}</p>
                    </div>
                    <div style='text-align: center; padding: 15px; font-size: 12px; color: #777;'>
                        <p style='margin: 0;'>This email was sent automatically. Please do not reply.</p>
                    </div>
                </div>
            ";

            emailMessage.Body = new TextPart("html")
            {
                Text = formattedBody
            };

            using var smtp = new SmtpClient(); // Use MailKit's SmtpClient
            await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), false);
            await smtp.AuthenticateAsync(emailSettings["SmtpUsername"], emailSettings["SmtpPassword"]);
            await smtp.SendAsync(emailMessage);
            await smtp.DisconnectAsync(true);
        }
    }
}
