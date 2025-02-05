using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
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
        public IActionResult GetAllUsers()
        {
            return Ok(new { message = "This route is accessible to all authenticated users." });
        }
    }
}
