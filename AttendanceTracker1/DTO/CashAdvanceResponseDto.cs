using AttendanceTracker1.Models;

namespace AttendanceTracker1.DTO
{
    public class CashAdvanceResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public decimal Amount { get; set; }
        public DateTime NeededDate { get; set; }
        public int MonthsToPay { get; set; }
        public List<CashAdvancePaymentSchedule> PaymentSchedule { get; set; }
        public string RequestStatus { get; set; }
        public DateTime RequestDate { get; set; }
        public string Reason { get; set; }
        public int? ReviewedBy { get; set; }
        public string ApproverName { get; set; }
        public string RejectionReason { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
