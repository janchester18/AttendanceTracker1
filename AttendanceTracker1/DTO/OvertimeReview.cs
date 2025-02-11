using AttendanceTracker1.Models;
using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class OvertimeReview
    {
        [Required]
        public OvertimeRequestStatus Status { get; set; } // Approved or Rejected

        [Required]
        public int ReviewedBy { get; set; } // Admin ID
    }
}
