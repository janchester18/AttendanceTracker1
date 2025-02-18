using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("admin")]
        [Authorize(Roles = "Admin")] // 🔹 Only Admins can access this
        public IActionResult GetAdminData()
        {
            return Ok(new { message = "Hello, Admin! This is a protected admin route." });
        }

        [HttpGet("employee")]
        [Authorize(Roles = "Employee")] // 🔹 Only Employees can access this
        public IActionResult GetEmployeeData()
        {
            return Ok(new { message = "Hello, Employee! This is a protected employee route." });
        }

        [HttpGet("all")]
        [Authorize] // 🔹 Any authenticated user (Admin or Employee) can access
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    Created = u.Created,
                    Updated = u.Updated,
                    AccumulatedOvertime = u.AccumulatedOvertime
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
