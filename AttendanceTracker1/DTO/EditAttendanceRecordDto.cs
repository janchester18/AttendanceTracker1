using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class EditAttendanceRecordDto
    {
        [Required]
        public DateTime ClockIn { get; set; }
        public DateTime? BreakStart { get; set; }
        public DateTime? BreakFinish { get; set; }
        [Required]
        public DateTime ClockOut { get; set; }
    }
}
