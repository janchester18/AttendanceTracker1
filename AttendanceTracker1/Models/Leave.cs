using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceTracker1.Models
{
    public class Leave
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [NotMapped]
        public int DaysCount => (EndDate - StartDate).Days + 1;
        [Required]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        [Required]
        public LeaveType Type { get; set; }
        public string? Reason { get; set; }
        public int? ReviewedBy { get; set; } 

        [ForeignKey("ReviewedBy")]
        public virtual User? Approver { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
    }

    public enum LeaveType
    {
        Vacation = 1,
        SickLeave = 2,
        PersonalLeave = 3,
        Bereavement = 4,
        Maternity = 5,
        Paternity = 6
    }

    public enum LeaveStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
