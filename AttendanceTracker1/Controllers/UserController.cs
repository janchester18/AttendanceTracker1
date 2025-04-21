using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AttendanceTracker1.Models.User;

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
                var totalRecords = await _context.Users.Where(t => t.SystemUserType == "Attendance").CountAsync();

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
                    .Where(u => u.SystemUserType == "Attendance")
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
                        Team = _context.UserTeams
                            .Where(ut => ut.UserId == u.Id)
                            .Select(ut => ut.Team.Name)
                            .FirstOrDefault(),
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
                        )),
                        u.Mpl,
                        VisibilityStatus = u.VisibilityStatus.ToString(),
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

        [HttpGet("cash-advance")]
        [Authorize] // Any authenticated user (Admin or Employee) can access
        public async Task<IActionResult> GetAllUsersCashAdvance(int page = 1, int pageSize = 10)
        {
            try
            {
                var skip = (page - 1) * pageSize;
                var totalRecords = await _context.Users.Where(t => t.SystemUserType == "Cash Advance").CountAsync();

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
                    .Where(u => u.SystemUserType == "Cash Advance")
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
                        Team = _context.UserTeams
                            .Where(ut => ut.UserId == u.Id)
                            .Select(ut => ut.Team.Name)
                            .FirstOrDefault(),
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
                        )),
                        u.Mpl,
                        VisibilityStatus = u.VisibilityStatus.ToString(),
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

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                // Update only if fields are provided
                if (dto.Name != null) user.Name = dto.Name;

                if (dto.Email != null) user.Email = dto.Email;

                if (dto.Phone != null) user.Phone = dto.Phone;

                if (dto.Role != null) user.Role = dto.Role;

                if (!string.IsNullOrWhiteSpace(dto.NewPassword)) user.SetPassword(dto.NewPassword);

                user.Updated = DateTime.Now;

                // ✅ Update team assignment if provided
                if (dto.TeamId.HasValue)
                {
                    var teamExists = await _context.Teams.AnyAsync(t => t.Id == dto.TeamId.Value);
                    if (!teamExists)
                        return BadRequest(ApiResponse<object>.Failed("Invalid Team ID."));

                    // Remove old assignment
                    var currentUserTeams = _context.UserTeams.Where(ut => ut.UserId == id);
                    _context.UserTeams.RemoveRange(currentUserTeams);

                    // Assign new team
                    var newAssignment = new UserTeam
                    {
                        UserId = id,
                        TeamId = dto.TeamId.Value,
                        AssignedAt = DateTime.UtcNow
                    };
                    _context.UserTeams.Add(newAssignment);
                }

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success(null, "Updated successfully."));
            }

            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        // PUT: api/Users/5
        [HttpPut("cash-advance/{id}")]
        public async Task<IActionResult> CashAdvanceUpdateUser(int id, [FromBody] UpdateCaUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Ok(ApiResponse<object>.Failed("Validation failed"));

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                // Update only if fields are provided
                if (dto.Name != null) user.Name = dto.Name;

                if (dto.Email != null) user.Email = dto.Email;

                if (dto.Phone != null) user.Phone = dto.Phone;

                if (dto.Role != null) user.Role = dto.Role;

                if (!string.IsNullOrWhiteSpace(dto.NewPassword)) user.SetPassword(dto.NewPassword);

                user.Updated = DateTime.Now;

                // ✅ Update team assignment if provided
                if (dto.TeamId.HasValue)
                {
                    var teamExists = await _context.Teams.AnyAsync(t => t.Id == dto.TeamId.Value);
                    if (!teamExists)
                        return Ok(ApiResponse<object>.Failed("Invalid Team ID."));

                    // Remove old assignment
                    var currentUserTeams = _context.UserTeams.Where(ut => ut.UserId == id);
                    _context.UserTeams.RemoveRange(currentUserTeams);

                    // Assign new team
                    var newAssignment = new UserTeam
                    {
                        UserId = id,
                        TeamId = dto.TeamId.Value,
                        AssignedAt = DateTime.UtcNow
                    };
                    _context.UserTeams.Add(newAssignment);
                }

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success(null, "Updated successfully."));
            }

            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("disable/{id}")]
        public async Task<IActionResult> DisableUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                // Update only if fields are provided
                if (user.VisibilityStatus == UserVisibilityStatus.Disabled) return Ok(ApiResponse<object>.Failed("Visibility status is already disabled"));

                user.VisibilityStatus = UserVisibilityStatus.Disabled;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success(user, "Visibility updated successfully."));
            }

            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("enable/{id}")]
        public async Task<IActionResult> EnableUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                // Update only if fields are provided
                if (user.VisibilityStatus == UserVisibilityStatus.Enabled) return Ok(ApiResponse<object>.Failed("Visibility status is already enabled"));

                user.VisibilityStatus = UserVisibilityStatus.Enabled;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.Success(user, "Visibility updated successfully."));
            }

            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
