namespace AttendanceTracker1.DTO
{
    public class LogResponseDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Type { get; set; }
    }
}
