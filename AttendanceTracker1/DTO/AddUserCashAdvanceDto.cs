﻿using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class AddUserCashAdvanceDto
    {
        [Required]
        public required string Name { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, Phone]
        public required string Phone { get; set; }

        [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password { get; set; }

        [RegularExpression("^(Admin|Employee|Supervisor)$", ErrorMessage = "Role must be either 'Admin', 'Supervisor', or 'Employee'.")]
        public string? Role { get; set; }
        [Required]
        public int TeamId { get; set; }
    }
}
