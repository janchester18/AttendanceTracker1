using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class UserListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "Employee"; // Default role
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; }
        public double AccumulatedOvertime { get; set; } = 0.0;
    }
}
