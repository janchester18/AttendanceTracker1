using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceTracker1.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public string Title { get; set; } // The title of the notification

        [Required]
        public string Message { get; set; } // The message content of the notification

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // The time when the notification was created

        public DateTime? ReadAt { get; set; } // The time when the notification was read

        [Required]
        public bool IsRead { get; set; } = false; // Indicates whether the notification has been read

        public string Type { get; set; } // The type of notification (e.g., "Holiday", "Leave", "Overtime")

        public string? Link { get; set; } // Optional link to more details or related action
        public VisibilityStatus Status { get; set; } = VisibilityStatus.Enabled;

        // Method to mark the notification as read
        public void MarkAsRead()
        {
            IsRead = true;
        }

        public enum VisibilityStatus
        {
            Disabled = 0,
            Enabled = 1,
        }
    }
}
