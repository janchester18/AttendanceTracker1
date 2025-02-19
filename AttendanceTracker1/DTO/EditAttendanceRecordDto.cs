using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AttendanceTracker1.DTO
{
    public class EditAttendanceRecordDto
    {
        [DataType(DataType.DateTime)]
        [JsonPropertyName("clockIn")]
        public string? ClockIn { get; set; }
        public DateTime? BreakStart { get; set; }
        public DateTime? BreakFinish { get; set; }


        [DataType(DataType.DateTime)]
        [JsonPropertyName("clockOut")]
        public string? ClockOut { get; set; }
    }
}
