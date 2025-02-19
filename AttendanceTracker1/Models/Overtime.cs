using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceTracker1.Models
{
    public class Overtime
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User? User { get; set; } // Employee requesting overtime

        [Required]
        public DateTime Date { get; set; } // Overtime Date

        [Required]
        public TimeSpan StartTime { get; set; } // Overtime Start Time

        [Required]
        public TimeSpan EndTime { get; set; } // Overtime End Time

        [Required]
        public string Reason { get; set; } // Justification for Overtime

        [Required]
        public OvertimeRequestStatus Status { get; set; } = OvertimeRequestStatus.Pending;

        public int? ReviewedBy { get; set; } // Nullable for pending requests

        [ForeignKey("ReviewedBy")]
        public virtual User? Approver { get; set; } // Approver of overtime request
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Auto-set on create

        public DateTime? UpdatedAt { get; set; } // Nullable, updated when modified

        [NotMapped] // Prevents mapping to the database
        public TimeSpan? WorkDuration => (StartTime < EndTime) ? EndTime - StartTime : null;

        [NotMapped] // Prevents mapping to the database
        public string FormattedWorkDuration
        {
            get
            {
                if (!WorkDuration.HasValue)
                    return "N/A";

                int hours = (int)WorkDuration.Value.TotalHours;
                int minutes = WorkDuration.Value.Minutes;

                return $"{hours}h {minutes}m";
            }
        }

        // 🔹 Validation: Ensure StartTime < EndTime
        public bool IsValidOvertime() => StartTime < EndTime;
    }

    public enum OvertimeRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
