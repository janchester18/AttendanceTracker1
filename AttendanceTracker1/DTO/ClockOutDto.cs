using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AttendanceTracker1.DTO
{
    public class ClockOutDto
    {
        [DataType(DataType.DateTime)]
        public string? ClockOut { get; set; }
        [JsonPropertyName("clockOutLatitude")]
        public double? ClockOutLatitude { get; set; }

        [JsonPropertyName("clockOutLongitude")]
        public double? ClockOutLongitude { get; set; }
    }
}
