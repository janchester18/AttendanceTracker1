using System.Text.Json.Serialization;

namespace AttendanceTracker1.DTO
{
    public class ClockOutDto
    {
        [JsonPropertyName("clockOutLatitude")]
        public double? ClockOutLatitude { get; set; }

        [JsonPropertyName("clockOutLongitude")]
        public double? ClockOutLongitude { get; set; }
    }
}
