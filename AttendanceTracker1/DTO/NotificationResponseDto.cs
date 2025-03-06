using AttendanceTracker1.Models;
using System.Text.Json.Serialization;

namespace AttendanceTracker1.DTO
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [JsonIgnore]
        public virtual User? User { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; } 
        public string Message { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; } 
        public bool IsRead { get; set; } = false; 
        public string Type { get; set; } 
        public string? Link { get; set; } 
        public string Status { get; set; }
    }
}
