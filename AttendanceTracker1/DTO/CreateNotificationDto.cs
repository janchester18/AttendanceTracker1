namespace AttendanceTracker1.DTO
{
    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? Type { get; set; }
        public string? Link { get; set; }
    }
}
