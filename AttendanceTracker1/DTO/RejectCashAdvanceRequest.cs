using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class RejectCashAdvanceRequest
    {
        [Required]
        public string RejectionReason { get; set; }
    }
}
