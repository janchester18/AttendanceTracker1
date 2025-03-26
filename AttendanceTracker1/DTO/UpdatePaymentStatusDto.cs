using AttendanceTracker1.Models;
using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class UpdatePaymentStatusDto
    {
        [Required]
        public CashAdvancePaymentStatus Status { get; set; }
    }
}
