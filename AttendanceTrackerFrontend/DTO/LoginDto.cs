using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AttendanceTrackerFrontend.DTO
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
