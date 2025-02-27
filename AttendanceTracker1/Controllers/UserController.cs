using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
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

        [HttpGet]
        [Authorize] // 🔹 Any authenticated user (Admin or Employee) can access
        public async Task<IActionResult> GetAllUsers(int page = 1, int pageSize = 10)
        {
            try
            {
                var skip = (page - 1) * pageSize;
                var totalRecords = await _context.Users.CountAsync();

                var users = await _context.Users
                    .Skip(skip) // Skip the records for the previous pages
                    .Take(pageSize) // Limit the number of records to the page size
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

                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var response = ApiResponse<object>.Success(new
                {
                    users = users,
                    totalRecords,
                    totalPages,
                    currentPage = page,
                    pageSize,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                }, "User list request successful.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }   
        }
    }
}
