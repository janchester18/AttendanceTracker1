using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceTracker1.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime ClockIn { get; set; }

        public DateTime? ClockOut { get; set; }

        // New properties for location tracking
        public double? ClockInLatitude { get; set; }
        public double? ClockInLongitude { get; set; }
        public double? ClockOutLatitude { get; set; }
        public double? ClockOutLongitude { get; set; }

        // New properties to record break times (optional)
        public DateTime? BreakStart { get; set; }
        public DateTime? BreakFinish { get; set; }

        /// <summary>
        /// Calculates the effective work duration. 
        /// If both ClockOut and break times are provided, it subtracts the break duration from the total time between ClockIn and ClockOut.
        /// </summary>
        [NotMapped]
        public TimeSpan? WorkDuration
        {
            get
            {
                // Ensure that ClockOut is available to calculate any duration
                if (!ClockOut.HasValue)
                    return null;

                // Calculate the total time between clock in and clock out
                TimeSpan totalDuration = ClockOut.Value - ClockIn;

                // If both break start and finish are provided, subtract the break duration
                if (BreakStart.HasValue && BreakFinish.HasValue)
                {
                    TimeSpan breakDuration = BreakFinish.Value - BreakStart.Value;
                    totalDuration = totalDuration - breakDuration;
                }

                return totalDuration;
            }
        }

        [Required]
        public AttendanceStatus Status { get; set; }  // Enum (Present, Absent, Late, OnLeave)

        public string? Remarks { get; set; }  // Optional comments

        public double LateDuration { get; set; } = 0.0;

        public double NightDifDuration { get; set; } = 0.0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Returns a formatted string representation of the work duration.
        /// </summary>
        public string FormattedWorkDuration
        {
            get
            {
                if (!WorkDuration.HasValue)
                    return "N/A";

                // Round the hours and minutes appropriately
                double totalHours = WorkDuration.Value.TotalHours;
                int hours = (int)totalHours;
                int minutes = WorkDuration.Value.Minutes;

                return $"{hours}h {minutes}m";
            }
        }

        /// <summary>
        /// Returns a formatted string representation of the break duration.
        /// </summary>
        public string FormattedBreakDuration
        {
            get
            {
                if (BreakStart.HasValue && BreakFinish.HasValue)
                {
                    TimeSpan breakDuration = BreakFinish.Value - BreakStart.Value;
                    int hours = (int)breakDuration.TotalHours;
                    int minutes = breakDuration.Minutes;
                    return $"{hours}h {minutes}m";
                }

                return "N/A";
            }
        }

        /// <summary>
        /// Returns a formatted string representation of the late duration.
        /// </summary>
        public string FormattedLateDuration
        {
            get
            {
                if (LateDuration <= 0)
                    return "On Time";

                int hours = (int)(LateDuration / 60);
                int minutes = (int)(LateDuration % 60);

                return $"{hours}h {minutes}m";
            }
        }
    }

    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        Late = 3,
        OnLeave = 4
    }
}
