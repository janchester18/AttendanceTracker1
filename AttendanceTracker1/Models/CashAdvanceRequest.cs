using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        [Range(500, 1000000, ErrorMessage = "Amount must be between 500 and 1,000,000.")]
        public decimal Amount { get; set; }

        [Required]

        public DateTime NeededDate { get; set; }

        [Required(ErrorMessage = "Months to pay is required.")]
        [Range(1, 60, ErrorMessage = "Months to pay must be between 1 and 60.")]
        public int MonthsToPay { get; set; }

        public List<CashAdvancePaymentSchedule>? PaymentSchedule { get; set; }

        [Required]
        public CashAdvanceRequestStatus Status { get; set; } = CashAdvanceRequestStatus.Pending; // pending, approved, rejected

        public string RequestStatus => Status.ToString();

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.Now; // Date of request

        [Required]
        public string Reason { get; set; }

        public int? ReviewedBy { get; set; }

        [ForeignKey("ReviewedBy")]
        [JsonIgnore]
        public virtual User? Approver { get; set; }

        public string? RejectionReason { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        [JsonIgnore]
        public virtual User User { get; set; } // Relationship with the User model
    }

    public enum CashAdvanceRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        ForEmployeeApproval = 4,
        Paid = 5,
        ForAdminApproval = 6
    }
}
