namespace AttendanceTracker1.DTO
{
    public class OvertimeResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty; // From User
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ApproverName { get; set; } // From Approver (if exists)
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
