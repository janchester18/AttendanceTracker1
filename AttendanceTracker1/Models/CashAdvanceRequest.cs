using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceTracker1.Models
{
    public class CashAdvanceRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; } 

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; } 

        [Required]
        public CashAdvanceRequestStatus Status { get; set; } = CashAdvanceRequestStatus.Pending; // pending, approved, rejected

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.Now; // Date of request

        [Required]
        public string Reason { get; set; }

        public int? ReviewedBy { get; set; }

        [ForeignKey("ReviewedBy")]
        public virtual User? Approver { get; set; }

        public string? RejectionReason { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } // Relationship with the User model
    }

    public enum CashAdvanceRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }
}
