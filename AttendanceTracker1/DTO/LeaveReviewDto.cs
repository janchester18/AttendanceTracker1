using AttendanceTracker1.Models;
using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class LeaveReviewDto
    {
        [Required]
        public LeaveStatus Status { get; set; } // Approved or Rejected

        [Required]
        public int ReviewedBy { get; set; } // Admin ID
        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}
