using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class OvertimeConfigDto
    {

        public double? OvertimeDailyMax { get; set; }
        public double? BreaktimeMax { get; set; }
        public TimeSpan? OfficeStartTime { get; set; }  // Office opening time
        public TimeSpan? OfficeEndTime { get; set; }    // Office closing time
        public TimeSpan? NightDifStartTime { get; set; }  // Office opening time
        public TimeSpan? NightDifEndTime { get; set; }    // Office closing time
    }
}
