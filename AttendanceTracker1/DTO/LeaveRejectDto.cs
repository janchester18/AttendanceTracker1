using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class LeaveRejectDto
    {
        [MaxLength(500)]
        [Required]
        public string RejectionReason { get; set; }
    }
}
