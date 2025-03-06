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
        public string StatusName { get; set; }
        public string TypeName { get; set; }
        public string? Reason { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ApproverName { get; set; } // Avoids cyclic reference
        public string? RejectionReason { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
