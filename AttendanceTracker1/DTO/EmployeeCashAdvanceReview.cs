using AttendanceTracker1.Models;
using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class EmployeeCashAdvanceReview
    {
        [Required]
        public CashAdvanceRequestStatus Status { get; set; }
    }
}
