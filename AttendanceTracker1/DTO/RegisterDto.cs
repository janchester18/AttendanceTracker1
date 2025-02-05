using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class RegisterDto
    {
        [Required]
        public required string Name { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, Phone]
        public required string Phone { get; set; }

        [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password { get; set; }

        [Required]
        [RegularExpression("^(Admin|Employee)$", ErrorMessage = "Role must be either 'Admin' or 'Employee'.")]
        public required string Role { get; set; } // Only allows "Admin" or "Employee"
    }
}
