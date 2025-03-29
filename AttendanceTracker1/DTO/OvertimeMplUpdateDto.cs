using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class OvertimeMplUpdateDto
    {
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The number of MPLs must be 0 or greater.")]
        public int MPLConverted { get; set; }
    }
}
