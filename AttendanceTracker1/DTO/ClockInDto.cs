using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AttendanceTracker1.DTO
{
    public class ClockInDto
    {
        [Required]
        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [DataType(DataType.Date)]
        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [DataType(DataType.DateTime)]
        [JsonPropertyName("clockIn")]
        public string? ClockIn { get; set; }

        [DataType(DataType.DateTime)]
        [JsonPropertyName("clockOut")]
        public string? ClockOut { get; set; }

        [JsonPropertyName("status")]
        public int? Status { get; set; }

        [StringLength(255)]
        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }

        // New properties for location tracking
        [JsonPropertyName("clockInLatitude")]
        public double? ClockInLatitude { get; set; }

        [JsonPropertyName("clockInLongitude")]
        public double? ClockInLongitude { get; set; }

        [JsonPropertyName("lateDuration")]
        public double LateDuration { get; set; } = 0.0;
    }
}
