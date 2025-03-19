using AttendanceTracker1.Models;
using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class UpdateLeaveDto
    {
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; } // Leave start date

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; } // Leave end date

        [StringLength(500, ErrorMessage = "Reason should not exceed 500 characters.")]
        public string? Reason { get; set; } // Reason for leave

        [EnumDataType(typeof(LeaveType))]
        public LeaveType? Type { get; set; } // Type of leave (e.g., Sick, Vacation, etc.)
    }
}
