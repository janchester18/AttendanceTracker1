using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize] // Any authenticated user (Admin or Employee) can access
        public async Task<IActionResult> GetAllUsers(int page = 1, int pageSize = 10)
        {
            try
            {
                var skip = (page - 1) * pageSize;
                var totalRecords = await _context.Users.CountAsync();

                // Calculate the cutoff period:
                // - If today is on or after the 16th, the period is from the 16th of this month to the 15th of next month.
                // - If today is before the 16th, the period is from the 16th of last month to the 15th of this month.
                var today = DateTime.UtcNow;
                DateTime startDate, endDate;
                if (today.Day >= 16)
                {
                    startDate = new DateTime(today.Year, today.Month, 16);
                    endDate = today.Month == 12
                        ? new DateTime(today.Year + 1, 1, 15)
                        : new DateTime(today.Year, today.Month + 1, 15);
                }
                else
                {
                    startDate = new DateTime(today.Year, today.Month, 16).AddMonths(-1);
                    endDate = new DateTime(today.Year, today.Month, 15);
                }

                // Retrieve users with their overtime hours minus those already converted.
                var users = await _context.Users
                    .OrderBy(u => u.Role)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.Id,
                        u.Name,
                        u.Email,
                        u.Phone,
                        u.Role,
                        u.Created,
                        u.Updated,
                        OvertimeHours = Math.Floor((
                            // Total overtime hours (converted from minutes to hours)
                            (_context.Attendances
                                .Where(a => a.UserId == u.Id && a.Date >= startDate && a.Date < endDate)
                                .Sum(a => (double?)a.OvertimeDuration / 60) ?? 0)
                            -
                            // Subtract the hours that have been converted to MPL
                            (_context.OvertimeMpls
                                .Where(o => o.UserId == u.Id && o.CutoffStartDate == startDate && o.CutoffEndDate == endDate)
                                .Sum(o => (double?)o.MPLConverted * 8) ?? 0)
                        ))
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var response = ApiResponse<object>.Success(new
                {
                    users,
                    totalRecords,
                    totalPages,
                    currentPage = page,
                    pageSize,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1,
                    startDate,
                    endDate
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
