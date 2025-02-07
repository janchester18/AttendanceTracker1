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

        [NotMapped] 
        public TimeSpan? WorkDuration => ClockOut.HasValue ? ClockOut - ClockIn : null;

        [Required]
        public AttendanceStatus Status { get; set; }  // Enum (Present, Absent, Late, OnLeave)

        public string? Remarks { get; set; }  // Optional comments
    }

    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        OnLeave
    }
}
