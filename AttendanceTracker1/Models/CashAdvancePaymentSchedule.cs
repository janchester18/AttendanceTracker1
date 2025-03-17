using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AttendanceTracker1.Models
{
    public class CashAdvancePaymentSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CashAdvanceRequestId { get; set; }

        [ForeignKey("CashAdvanceRequestId")]
        [JsonIgnore]
        public virtual CashAdvanceRequest CashAdvanceRequest { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; } // Monthly installment amount

        [Required]
        public DateTime PaymentDate { get; set; } // Scheduled date of payment

        [JsonIgnore]
        public CashAdvancePaymentStatus Status { get; set; } = CashAdvancePaymentStatus.PendingApproval; // Track if this installment has been paid

        public string PaymentStatus => Status.ToString();

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    public enum CashAdvancePaymentStatus
    {
        PendingApproval = 1,
        Unpaid = 2,
        Paid = 3,
        ForEmployeeApproval = 4,
        Rejected = 5,
    }
}
