using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class UpdateCaUserDto
    {
        // Optional, but if provided must follow name rules (you can customize)
        public string? Name { get; set; }

        // Optional, validate if provided
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? Phone { get; set; }

        [RegularExpression("^(Admin|Employee|Supervisor)$", ErrorMessage = "Role must be either 'Admin', 'Supervisor', or 'Employee'.")]
        public string? Role { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string? NewPassword { get; set; }
        public int? TeamId { get; set; } // Optional, update only if provided
    }
}
