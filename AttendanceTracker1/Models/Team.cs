using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.Models
{
    public class Team
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = "No Team";

        public ICollection<UserTeam>? UserTeams { get; set; }
    }
}
