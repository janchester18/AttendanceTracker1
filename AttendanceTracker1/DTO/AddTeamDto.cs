using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class AddTeamDto
    {
        [Required(ErrorMessage = "Team name is required.")]
        [MinLength(3, ErrorMessage = "Team name must be at least 3 characters")]
        public string Name { get; set; }
    }
}
