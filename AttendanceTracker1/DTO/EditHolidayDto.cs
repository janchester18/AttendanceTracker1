using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class EditHolidayDto
    {
        public string? Name { get; set; } // Example: "Christmas", "Independence Day"

        public DateTime? Date { get; set; } // The actual holiday date

        [StringLength(10)]
        public string? IsPaid { get; set; } // Example: "Yes", "No", "Optional"

        [StringLength(10)]
        public string? IsNational { get; set; } // Example: "Yes", "No", "Local"

        [StringLength(20)]
        public string? Type { get; set; } // Example: "Public", "Religious", "Company Holiday"
    }
}
