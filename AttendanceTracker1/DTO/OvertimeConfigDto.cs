using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class OvertimeConfigDto
    {

        public double? OvertimeDailyMax { get; set; }
        public double? BreakMax { get; set; }  
        public TimeSpan? OfficeStartTime { get; set; }  
        public TimeSpan? OfficeEndTime { get; set; }    
        public TimeSpan? NightDifStartTime { get; set; }  
        public TimeSpan? NightDifEndTime { get; set; }    
    }
}
