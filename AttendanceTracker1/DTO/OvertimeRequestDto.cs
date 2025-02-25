using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class OvertimeRequestDto
    {
        
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } // Overtime date (YYYY-MM-DD format)

        [Required(ErrorMessage = "Start Time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; } // Overtime start time (HH:mm format)

        [Required(ErrorMessage = "End Time is required.")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; } // Overtime end time (HH:mm format)

        [Required(ErrorMessage = "Reason is required.")]
        [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters.")]
        public string Reason { get; set; } // Justification for overtime

        [Required(ErrorMessage = "Expected output is required.")]
        [StringLength(500, ErrorMessage = "Expected output must not exceed 500 characters.")]
        public string ExpectedOutput { get; set; } // Expected Output for the Overtime
    }
}
