using AttendanceTracker1.Models;

namespace AttendanceTracker1.DTO
{
    public class LeaveResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; } // Prevents cyclic reference
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysCount { get; set; }
        public LeaveStatus Status { get; set; }
        public LeaveType Type { get; set; }
        public string? Reason { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ApproverName { get; set; } // Avoids cyclic reference
        public DateTime? CreatedDate { get; set; }
    }
}
