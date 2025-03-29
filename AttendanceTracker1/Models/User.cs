using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceTracker1.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-generate ID
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty; 

        [Required]
        [RegularExpression("^(Admin|Employee)$", ErrorMessage = "Role must be either 'Admin' or 'Employee'.")]
        public string Role { get; set; } = "Employee"; // Default role
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; }
        public double Mpl { get; set; } = 0.0;
        public ICollection<Attendance>? Attendances { get; set; }
        public ICollection<Leave>? Leaves { get; set; }
        public ICollection<Leave>? Approvals { get; set; }
        public ICollection<Overtime>? OvertimeRequests { get; set; }
        public ICollection<Overtime>? OvertimeApprovals { get; set; }

        // 🔹 Hash password before saving
        public void SetPassword(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

        // 🔹 Verify password
        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }
    }
}
