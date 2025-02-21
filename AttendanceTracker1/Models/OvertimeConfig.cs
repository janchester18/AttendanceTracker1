using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.Models
{
    public class OvertimeConfig
    {
        public int Id { get; set; }
        [Required]
        public double OvertimeDailyMax { get; set; }
        [Required]
        public double BreaktimeMax { get; set; }
        [Required]
        public TimeSpan OfficeStartTime { get; set; }  // Office opening time
        [Required]
        public TimeSpan OfficeEndTime { get; set; }    // Office closing time
        [Required]
        public TimeSpan NightDifStartTime { get; set; }  // Office opening time
        [Required]
        public TimeSpan NightDifEndTime { get; set; }    // Office closing time
        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}
