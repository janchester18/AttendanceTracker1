using System.ComponentModel.DataAnnotations;
using AttendanceTracker1.Models;

namespace AttendanceTracker1.DTO
{
    public class RequestLeaveDto
    {
            [Required]
            public int UserId { get; set; } // The ID of the user requesting leave

            [Required]
            [DataType(DataType.Date)]
            public DateTime StartDate { get; set; } // Leave start date

            [Required]
            [DataType(DataType.Date)]
            public DateTime EndDate { get; set; } // Leave end date

            [StringLength(500, ErrorMessage = "Reason should not exceed 500 characters.")]
            public string? Reason { get; set; } // Reason for leave

            [Required]
            [EnumDataType(typeof(LeaveType))]
            public LeaveType Type { get; set; } // Type of leave (e.g., Sick, Vacation, etc.)

            public bool RequiresApproval { get; set; } = true; // Whether approval is needed

            public int? ReviewedBy { get; set; } // Approver ID (can be null initially)
    }
}
