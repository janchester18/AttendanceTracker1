using AttendanceTracker1.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class CashAdvanceRequestDto
    {
        [Required]
        [Range(typeof(decimal), "0.01", "9999999999999999.99", ErrorMessage = "Amount must be a positive number.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime NeededDate { get; set; }

        [Required(ErrorMessage = "Months to pay is required.")]
        [Range(1, 60, ErrorMessage = "Months to pay must be between 1 and 60.")]
        public int MonthsToPay { get; set; }

        [Required]
        public List<DateTime> PaymentDates { get; set; } // Employee-specified dates

        [Required]
        public string Reason { get; set; }


    }
}
