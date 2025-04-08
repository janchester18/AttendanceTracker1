﻿using System.ComponentModel.DataAnnotations;

namespace AttendanceTracker1.DTO
{
    public class UpdateUserDto
    {
        // Optional, but if provided must follow name rules (you can customize)
        public string? Name { get; set; }

        // Optional, validate if provided
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? Phone { get; set; }

        [RegularExpression("^(Admin|Employee)$", ErrorMessage = "Role must be either 'Admin' or 'Employee'.")]
        public string? Role { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string? NewPassword { get; set; }
    }
}
