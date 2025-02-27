using System.Security.Claims;
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
                        a.FormattedLateDuration,
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
                a.FormattedLateDuration,
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
                a.FormattedLateDuration,
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
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                var userId = int.Parse(userIdClaim);

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

                if (DateTime.TryParse(clockInDto.ClockIn, out DateTime parsedClockIn))
                {
                    attendance.ClockIn = parsedClockIn;
                }
                else
                {
                    return BadRequest("Invalid clock-in time format.");
                }

                var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();
                var clockInTime = attendance.ClockIn.TimeOfDay;

                string message = "Clock-in recorded successfully.";

                var officeStartDateTime = today.Add(config.OfficeStartTime);
                var lateDuration = (attendance.ClockIn - officeStartDateTime).TotalMinutes; // Store in minutes

                if (lateDuration > 0)
                {
                    attendance.Status = AttendanceStatus.Late;
                    attendance.LateDuration = lateDuration; // Store in minutes

                    int lateHours = (int)(lateDuration / 60);
                    int lateMinutes = (int)(lateDuration % 60);

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
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                var userId = int.Parse(userIdClaim);

                var attendance = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == DateTime.Today);

                if (attendance == null) return NotFound("Attendance not found.");
                if (attendance.ClockOut.HasValue) return BadRequest("You have already clocked out.");

                if (DateTime.TryParse(clockOutDto.ClockOut, out DateTime parsedClockOut))
                {
                    attendance.ClockOut = parsedClockOut;
                }
                else
                {
                    return BadRequest("Invalid clock-out time format.");
                }

                attendance.ClockOutLatitude = clockOutDto.ClockOutLatitude;
                attendance.ClockOutLongitude = clockOutDto.ClockOutLongitude;

                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound("User not found.");

                var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();
                if (config == null) return NotFound("Overtime configuration not found.");

                // Break time calculation in minutes
                int breakMinutes = (attendance.BreakStart.HasValue && attendance.BreakFinish.HasValue)
                    ? (int)(attendance.BreakFinish.Value - attendance.BreakStart.Value).TotalMinutes
                    : 0;

                // Work period
                DateTime workStart = attendance.ClockIn;
                DateTime workEnd = attendance.ClockOut.Value;

                // Night differential calculation
                DateTime nightStart = DateTime.Today.Add(config.NightDifStartTime);
                DateTime nightEnd = config.NightDifEndTime > config.NightDifStartTime
                    ? DateTime.Today.Add(config.NightDifEndTime)
                    : DateTime.Today.AddDays(1).Add(config.NightDifEndTime);

                // Calculate effective night differential period
                DateTime effectiveNightStart = workStart > nightStart ? workStart : nightStart;
                DateTime effectiveNightEnd = workEnd < nightEnd ? workEnd : nightEnd;

                int nightDifferentialMinutes = effectiveNightStart < effectiveNightEnd
                    ? (int)(effectiveNightEnd - effectiveNightStart).TotalMinutes
                    : 0;

                attendance.NightDifDuration = nightDifferentialMinutes;
                user.AccumulatedNightDifferential += nightDifferentialMinutes;

                // Total work duration in minutes
                int totalWorkMinutes = (int)(workEnd - workStart).TotalMinutes - breakMinutes;
                int regularWorkMinutes = (int)((config.OfficeEndTime - config.OfficeStartTime).TotalMinutes - config.BreaktimeMax);

                int actualOvertimeMinutes = Math.Max(0, totalWorkMinutes - regularWorkMinutes);

                var approvedOvertime = await _context.Overtimes
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.Date.Date == attendance.ClockIn.Date && o.Status == OvertimeRequestStatus.Approved);

                int approvedOvertimeMinutes = 0;

                if (approvedOvertime != null)
                {
                    approvedOvertimeMinutes = (int)Math.Min(actualOvertimeMinutes,
                        Math.Min((approvedOvertime.EndTime - approvedOvertime.StartTime).TotalMinutes, config.OvertimeDailyMax));
                }

                user.AccumulatedOvertime += approvedOvertimeMinutes;

                await _context.SaveChangesAsync();

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Attendance")
                    .Information("{UserName} clocked out at {Time}", username, DateTime.Now);

                // Format Minutes to "Xh Ym"
                string FormatMinutes(double minutes) => $"{minutes / 60}h {minutes % 60}m";

                return Ok(ApiResponse<object>.Success(new
                {
                    message = "Clock-out recorded successfully.",
                    totalWorkTime = FormatMinutes(totalWorkMinutes),
                    approvedOvertime = approvedOvertime != null,
                    approvedOvertimeDuration = FormatMinutes(approvedOvertimeMinutes),
                    actualOvertimeWorked = FormatMinutes(actualOvertimeMinutes),
                    overtimeAdded = FormatMinutes(approvedOvertimeMinutes),
                    nightDifferential = FormatMinutes(nightDifferentialMinutes),
                    newAccumulatedOvertime = FormatMinutes(user.AccumulatedOvertime),
                    newAccumulatedNightDifferential = FormatMinutes(user.AccumulatedNightDifferential)
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("start-break")]
        [Authorize]
        public async Task<IActionResult> StartBreak()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                var userId = int.Parse(userIdClaim);

                var user = await _context.Users.FindAsync(userId);

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
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                var userId = int.Parse(userIdClaim);

                var user = await _context.Users.FindAsync(userId);

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
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var adminName = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(adminName) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                var adminId = int.Parse(userIdClaim);
                var admin = await _context.Users.FindAsync(adminId);

                var attendance = await _context.Attendances.FindAsync(id);
                if (attendance == null)
                {
                    return BadRequest("Attendance Record not Found");
                }

                // ✅ Parse the ClockIn and ClockOut strings before updating
                if (!string.IsNullOrWhiteSpace(updatedAttendance.ClockIn) && DateTime.TryParse(updatedAttendance.ClockIn, out DateTime clockInValue))
                {
                    attendance.ClockIn = clockInValue;
                }

                if (!string.IsNullOrWhiteSpace(updatedAttendance.ClockOut) && DateTime.TryParse(updatedAttendance.ClockOut, out DateTime clockOutValue))
                {
                    attendance.ClockOut = clockOutValue;
                }

                // ✅ Assign BreakStart and BreakFinish only if they are provided
                attendance.BreakStart = updatedAttendance.BreakStart ?? attendance.BreakStart;
                attendance.BreakFinish = updatedAttendance.BreakFinish ?? attendance.BreakFinish;

                await _context.SaveChangesAsync();

                // ✅ Dynamically build response
                var response = new Dictionary<string, object>
                {
                    { "message", "Attendance Record updated successfully." },
                    { "totalBreakHours", attendance.FormattedBreakDuration },
                    { "totalWorkHours", attendance.FormattedWorkDuration }
                };

                new Dictionary<string, DateTime?>
                {
                    { "newClockIn", attendance.ClockIn },
                    { "newClockOut", attendance.ClockOut },
                    { "newBreakStart", attendance.BreakStart },
                    { "newBreakFinish", attendance.BreakFinish }
                }
                .Where(pair => pair.Value.HasValue)
                .ToList()
                .ForEach(pair => response.Add(pair.Key, pair.Value));

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Attendance")
                    .Information("Attendance {Id} has been edited by {AdminName}", id, adminName);

                return Ok(ApiResponse<object>.Success(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
