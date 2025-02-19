﻿using System.Security.Claims;
using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(ApplicationDbContext context, ILogger<AttendanceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAttendances(int page = 1, int pageSize = 10)
        {
            try
            {
                var skip = (page - 1) * pageSize;

                var totalRecords = await _context.Attendances.CountAsync();

                var attendances = await _context.Attendances
                    .Include(a => a.User)
                    .OrderBy(a => a.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        a.Id, // Attendance ID
                        a.UserId,
                        User = new
                        {
                            a.User.Name,
                            a.User.Email
                        },
                        a.Date,
                        a.ClockIn,
                        a.ClockOut,
                        a.BreakStart,
                        a.BreakFinish,
                        a.FormattedWorkDuration,
                        Status = a.Status.ToString(),
                        a.Remarks
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                // Wrap the result in ApiResponse
                var response = ApiResponse<object>.Success(new
                {
                    attendances = attendances,
                    totalRecords,
                    totalPages,
                    currentPage = page,
                    pageSize,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                });

                return Ok(response); // Returning the standardized API response
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a failed response
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse); // Return internal server error with failed status
            }
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceByUser(int id)
        {
            try
            {
                var attendance = await _context.Attendances
            .Where(a => a.UserId == id)
            .Include(a => a.User)
            .Select(a => new
            {

                a.UserId,
                User = new
                {
                    a.User.Name,
                    a.User.Email
                },
                a.Date,
                a.ClockIn,
                a.ClockOut,
                a.BreakStart,
                a.BreakFinish,
                a.FormattedWorkDuration,
                Status = a.Status.ToString(),
                a.Remarks
            })
                .ToListAsync();

                return Ok(ApiResponse<object>.Success(attendance));
            }
            catch (Exception ex)
            {
                // Handle any exceptions and return a failed response
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse); // Return internal server error with failed status
            }
            
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceByAttendanceId(int id)
        {
            try
            {
                var attendance = await _context.Attendances
            .Where(a => a.Id == id)
            .Include(a => a.User)
            .Select(a => new
            {
                a.Id, // Attendance ID
                a.UserId,
                User = new
                {
                    a.User.Name,
                    a.User.Email
                },
                a.Date,
                a.ClockIn,
                a.ClockOut,
                a.BreakStart,
                a.BreakFinish,
                a.FormattedWorkDuration,
                Status = a.Status.ToString(),
                a.Remarks
            })
            .FirstOrDefaultAsync();

                if (attendance == null)
                {
                    return NotFound("Attendance record not found.");
                }

                return Ok(ApiResponse<object>.Success(attendance));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }

        [HttpPost("clockin")]
        [Authorize]
        public async Task<IActionResult> ClockIn([FromBody] ClockInDto clockInDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get the user ID from the JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                var userId = int.Parse(userIdClaim); // Convert string to integer if necessary

                // Retrieve the user from the database to get the username.
                var user = await _context.Users.FindAsync(userId);
                var username = user?.Name ?? "Unknown"; // Adjust property name if needed (e.g., "Name")

                var today = DateTime.Now.Date;

                var existingAttendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today);

                if (existingAttendance != null && existingAttendance.ClockIn != default(DateTime))
                {
                    return BadRequest("You have already clocked in today.");
                }

                DateTime? parsedClockOut = null;
                if (!string.IsNullOrEmpty(clockInDto.ClockOut) &&
                    DateTime.TryParse(clockInDto.ClockOut, out DateTime tempClockOut))
                {
                    parsedClockOut = tempClockOut;
                }

                var attendance = new Attendance
                {
                    UserId = userId,
                    Date = today,
                    ClockIn = DateTime.Now,
                    ClockOut = parsedClockOut,
                    Status = AttendanceStatus.Present,
                    Remarks = clockInDto.Remarks,
                    ClockInLatitude = clockInDto.ClockInLatitude,
                    ClockInLongitude = clockInDto.ClockInLongitude,
                };

                var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();
                var officeStartTime = config.OfficeStartTime;
                var clockInTime = attendance.ClockIn.TimeOfDay;

                string message = "Clock-in recorded successfully.";

                if (officeStartTime < clockInTime)
                {
                    attendance.Status = AttendanceStatus.Late;

                    double lateDurationInHours = (clockInTime - officeStartTime).TotalHours;
                    int lateHours = (int)Math.Floor(lateDurationInHours);
                    int lateMinutes = (int)Math.Round((lateDurationInHours - lateHours) * 60);

                    attendance.LateDuration = lateDurationInHours;
                    message = $"Clock-in recorded successfully. However, you are late by {lateHours}h {lateMinutes}m.";
                }

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();

                var response = ApiResponse<object>.Success(new
                {
                    message,
                    attendanceId = attendance.Id,
                    clockinLatitude = attendance.ClockInLatitude,
                    clockinLongitude = attendance.ClockInLongitude
                });

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Attendance")
                    .Information("{UserName} clocked in at {Time}", username, DateTime.Now);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("clockout")]
        [Authorize]
        public async Task<IActionResult> ClockOut([FromBody] ClockOutDto clockOutDto)
        {
            try
            {
                // Extract UserId from the JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                int userId = int.Parse(userIdClaim);

                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == DateTime.Today);

                if (attendance == null)
                    return NotFound("Attendance not found.");

                if (attendance.ClockOut.HasValue)
                    return BadRequest("You have already clocked out.");

                attendance.ClockOut = DateTime.Now;
                attendance.ClockOutLatitude = clockOutDto.ClockOutLatitude;
                attendance.ClockOutLongitude = clockOutDto.ClockOutLongitude;

                var user = await _context.Users.FindAsync(attendance.UserId);
                if (user == null)
                    return NotFound("User not found.");
                var username = user?.Name ?? "Unknown"; // Adjust property name if needed (e.g., "Name")

                var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();
                if (config == null)
                    return NotFound("Overtime configuration not found.");

                TimeSpan breakDuration = TimeSpan.Zero;

                if (attendance.BreakFinish.HasValue && attendance.BreakStart.HasValue)
                {
                    DateTime breakStart = attendance.BreakFinish.Value;
                    DateTime breakEnd = attendance.BreakStart.Value;

                    if (breakStart >= attendance.ClockIn && breakEnd <= attendance.ClockOut)
                    {
                        breakDuration = breakEnd - breakStart;
                    }
                }

                TimeSpan totalWorkDuration = (attendance.ClockOut.Value - attendance.ClockIn) - breakDuration;
                double totalWorkHours = totalWorkDuration.TotalHours;
                int totalWorkHoursInt = (int)totalWorkDuration.TotalHours;
                int totalWorkMinutes = (int)(totalWorkDuration.TotalMinutes % 60);

                double breakTimeHours = config.BreaktimeMax / 60.0;
                double regularWorkHours = (config.OfficeEndTime - config.OfficeStartTime).TotalHours - breakTimeHours;

                var approvedOvertime = await _context.Overtimes
                    .Where(o => o.UserId == user.Id
                        && o.Date.Date == attendance.ClockIn.Date
                        && o.Status == OvertimeRequestStatus.Approved)
                    .FirstOrDefaultAsync();

                double overtimeHours = 0;

                if (totalWorkHours > regularWorkHours && approvedOvertime != null)
                {
                    double approvedOvertimeDuration = (approvedOvertime.EndTime - approvedOvertime.StartTime).TotalHours;
                    double actualOvertimeWorked = totalWorkHours - regularWorkHours;

                    overtimeHours = Math.Min(actualOvertimeWorked, Math.Min(approvedOvertimeDuration, config.OvertimeDailyMax));

                    user.AccumulatedOvertime += overtimeHours;
                }

                double lateDurationInHours = attendance.LateDuration;
                int lateHours = (int)Math.Floor(lateDurationInHours);
                int lateMinutes = (int)Math.Round((lateDurationInHours - lateHours) * 60);

                await _context.SaveChangesAsync();

                var response = ApiResponse<object>.Success(new
                {
                    message = "Clock-out recorded successfully.",
                    lateDuration = $"{lateHours}h {lateMinutes}m",
                    totalWorkHours = $"{totalWorkHoursInt}h {totalWorkMinutes}m",
                    approvedOvertime = approvedOvertime != null,
                    approvedOvertimeDuration = approvedOvertime != null ? $"{approvedOvertime.EndTime.Subtract(approvedOvertime.StartTime).TotalHours:F2}h" : "0h",
                    actualOvertimeWorked = $"{(totalWorkHours - regularWorkHours):F2}h",
                    overtimeAdded = $"{overtimeHours:F2}h",
                    newAccumulatedOvertime = $"{Math.Floor(user.AccumulatedOvertime)}h {Math.Round((user.AccumulatedOvertime % 1) * 60)}m"
                });

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Attendance")
                    .Information("{UserName} clocked out at {Time}", username, DateTime.Now);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }

        [HttpPut("start-break")]
        [Authorize]
        public async Task<IActionResult> StartBreak()
        {
            try
            {
                // Extract UserId from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                int userId = int.Parse(userIdClaim);

                var user = await _context.Users.FindAsync(userId);
                var username = user?.Name ?? "Unknown";

                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == DateTime.Today);

                if (attendance == null)
                    return NotFound("Attendance not found.");

                if (attendance.ClockOut.HasValue)
                    return BadRequest("Cannot start break because you are already clocked out.");

                if (attendance.BreakStart.HasValue || attendance.BreakFinish.HasValue)
                    return BadRequest("Break has already been started or ended.");

                attendance.BreakStart = DateTime.Now;
                await _context.SaveChangesAsync();

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Attendance")
                    .Information("{UserName} started break at {Time}", username, DateTime.Now);

                return Ok(ApiResponse<object>.Success(new { message = "Break has started." }));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }


        [HttpPut("end-break")]
        [Authorize]
        public async Task<IActionResult> EndBreak()
        {
            try
            {
                // Extract UserId from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                int userId = int.Parse(userIdClaim);

                var user = await _context.Users.FindAsync(userId);
                var username = user?.Name ?? "Unknown";

                // Get today's attendance record for the user
                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == DateTime.Today);

                if (attendance == null)
                    return NotFound("Attendance not found.");

                if (!attendance.BreakStart.HasValue)
                    return BadRequest("Cannot end break because break has not been started.");

                if (attendance.ClockOut.HasValue)
                    return BadRequest("Cannot end break because you are already clocked out.");

                if (attendance.BreakFinish.HasValue)
                    return BadRequest("Break has already been ended.");

                attendance.BreakFinish = DateTime.Now;
                await _context.SaveChangesAsync();

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Attendance")
                    .Information("{UserName} ended break at {Time}", username, DateTime.Now);

                return Ok(ApiResponse<object>.Success(new
                {
                    message = "Break has ended.",
                    breakDuration = attendance.FormattedBreakDuration // Ensure this property exists in your model
                }));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }


        [HttpPut("edit-attendance/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditAttendanceRecord(int id, [FromBody] EditAttendanceRecordDto updatedAttendance)
        {
            try
            {
                // Extract UserId from JWT token
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                int adminId = int.Parse(adminIdClaim);

                var admin = await _context.Users.FindAsync(adminId);
                var adminName = admin?.Name ?? "Unknown";

                var attendance = await _context.Attendances.FindAsync(id);

                if (attendance == null)
                {
                    return BadRequest("Attendance Record not Found");
                }

                // Parse required fields
                if (!DateTime.TryParse(updatedAttendance.ClockIn, out DateTime clockInValue))
                {
                    return BadRequest("Invalid ClockIn value.");
                }
                if (!DateTime.TryParse(updatedAttendance.ClockOut, out DateTime clockOutValue))
                {
                    return BadRequest("Invalid ClockOut value.");
                }

                attendance.ClockIn = clockInValue;
                attendance.ClockOut = clockOutValue;

                if (updatedAttendance.BreakStart.HasValue)
                {
                    attendance.BreakStart = updatedAttendance.BreakStart;
                }

                if (updatedAttendance.BreakFinish.HasValue)
                {
                    attendance.BreakFinish = updatedAttendance.BreakFinish;
                }

                await _context.SaveChangesAsync();

                var response = new Dictionary<string, object>
            {
                { "message", "Attendance Record updated successfully." },
                { "newClockin", attendance.ClockIn },
                { "newClockout", attendance.ClockOut }
            };

                if (updatedAttendance.BreakStart.HasValue)
                {
                    response.Add("newBreakStart", attendance.BreakStart);
                }

                if (updatedAttendance.BreakFinish.HasValue)
                {
                    response.Add("newBreakFinish", attendance.BreakFinish);
                }

                response.Add("totalBreakHours", attendance.FormattedBreakDuration);
                response.Add("totalWorkHours", attendance.FormattedWorkDuration);

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Attendance")
                    .Information("Attendance {Id} has been edited by {AdminName}", id, adminName);

                return Ok(ApiResponse<object>.Success(response));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }
    }
}
