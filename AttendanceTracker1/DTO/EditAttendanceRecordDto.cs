using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AttendanceTracker1.DTO
{
    public class EditAttendanceRecordDto
    {
        [DataType(DataType.DateTime)]
        [JsonPropertyName("clockIn")]
        public string? ClockIn { get; set; }

        [DataType(DataType.DateTime)]
        [JsonPropertyName("breakStart")]
        public string? BreakStart { get; set; }

        [DataType(DataType.DateTime)]
        [JsonPropertyName("breakFinish")]
        public string? BreakFinish { get; set; }


        [DataType(DataType.DateTime)]
        [JsonPropertyName("clockOut")]
        public string? ClockOut { get; set; }
    }
}
