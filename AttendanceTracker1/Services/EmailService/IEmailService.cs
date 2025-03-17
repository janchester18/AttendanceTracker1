namespace AttendanceTracker1.Services.EmailService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string sender_email, string name, string subject, string message);
    }
}
