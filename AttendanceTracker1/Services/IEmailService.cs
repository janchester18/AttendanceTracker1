namespace AttendanceTracker1.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string sender_email, string name, string subject, string message);
    }
}
