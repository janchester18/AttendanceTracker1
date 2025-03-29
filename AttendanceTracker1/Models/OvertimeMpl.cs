using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.Identity.Client;

namespace AttendanceTracker1.Models
{
    public class OvertimeMpl
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalOvertimeHours { get; set; }

        [Required]
        public int MPLConverted { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ResidualOvertimeHours { get; set; } // Optional: leftover overtime hours

        [Required]
        public DateTime CutoffStartDate { get; set; }

        [Required]
        public DateTime CutoffEndDate { get; set; }

        public DateTime ConversionDate { get; set; } // Optional: when conversion occurred

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }
}
