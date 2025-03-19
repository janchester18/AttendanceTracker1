using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class UpdateOvertimeDto
    {
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; } // Overtime date (YYYY-MM-DD format)

        [DataType(DataType.Time)]
        public TimeSpan? StartTime { get; set; } // Overtime start time (HH:mm format)

        [DataType(DataType.Time)]
        public TimeSpan? EndTime { get; set; } // Overtime end time (HH:mm format)

        [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters.")]
        public string? Reason { get; set; } // Justification for overtime

        [StringLength(500, ErrorMessage = "Expected output must not exceed 500 characters.")]
        public string? ExpectedOutput { get; set; } // Expected Output for the Overtime
    }
}
