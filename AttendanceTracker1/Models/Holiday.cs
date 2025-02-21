using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.Models
{
    public class Holiday
    {
        [Key]
        public int Id { get; set; }  // Primary Key

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Example: "Christmas", "Independence Day"

        [Required]
        public DateTime Date { get; set; } // The actual holiday date

        [Required]
        [StringLength(10)]
        public string IsPaid { get; set; } // Example: "Yes", "No", "Optional"

        [Required]
        [StringLength(10)]
        public string IsNational { get; set; } // Example: "Yes", "No", "Local"

        [Required]
        [StringLength(20)]
        public string Type { get; set; } // Example: "Public", "Religious", "Company Holiday"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp

        public DateTime UpdatedAt { get; set; }
    }
}
