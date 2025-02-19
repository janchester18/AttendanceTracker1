using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        // Assuming Message and Level are always present.
        [Required]
        public string Message { get; set; }

        [Required]
        public string Level { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        // These columns might be null.
        public string? MessageTemplate { get; set; }

        public string? Exception { get; set; }
        public string? Type { get; set; }
    }
}
