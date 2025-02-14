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
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAttendances()
        {
            var attendances = await _context.Attendances
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
                    a.Status,        
                    a.Remarks       
                })
                .ToListAsync();

             
            return Ok(attendances);
        }

        [HttpGet("userId")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceByUser(int userId)
        {
            var attendance = await _context.Attendances
            .Where(a => a.UserId == userId)
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
                a.Status,       
                a.Remarks        
            })
                .ToListAsync();

            return Ok(attendance);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceByAttendanceId(int id)
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
                a.Status,       
                a.Remarks       
            })
            .FirstOrDefaultAsync();

            if (attendance == null)
            {
                return NotFound("Attendance record not found.");
            }

            return Ok(attendance);
        }

        [HttpPost("clockin")]
        [Authorize]
        public async Task<IActionResult> ClockIn([FromBody] ClockInDto clockInDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

        
            var today = DateTime.Now.Date;

           
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == clockInDto.UserId && a.Date == today);

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
                UserId = clockInDto.UserId,
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

                // Calculate late duration as double
                double lateDurationInHours = (clockInTime - officeStartTime).TotalHours;

                // Extract hours and minutes using rounding
                int lateHours = (int)Math.Floor(lateDurationInHours); // Whole hours
                int lateMinutes = (int)Math.Round((lateDurationInHours - lateHours) * 60); // Remaining minutes

                attendance.LateDuration = lateDurationInHours;

                message = $"Clock-in recorded successfully. However, you are late by {lateHours}h {lateMinutes}m.";
            }

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new 
            {
                message,
                attendanceId = attendance.Id,
                clockinLatitude = attendance.ClockInLatitude,
                clockinLongitude = attendance.ClockInLongitude
            });
        }

        [HttpPut("{id}/clockout")]
        [Authorize]
        public async Task<IActionResult> ClockOut(int id, [FromBody] ClockOutDto clockOutDto)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
                return NotFound("Attendance not found.");

            if (attendance.ClockOut.HasValue)
                return BadRequest("You have already clocked out.");

            if (DateTime.TryParse(clockOutDto.ClockOut, out DateTime parsedClockOut))
            {
                attendance.ClockOut = parsedClockOut;
            }
            else
            {
                return BadRequest("Invalid clock-out date format.");
            }

            attendance.ClockOutLatitude = clockOutDto.ClockOutLatitude;
            attendance.ClockOutLongitude = clockOutDto.ClockOutLongitude;

            // Fetch user and OvertimeConfig
            var user = await _context.Users.FindAsync(attendance.UserId);
            if (user == null)
                return NotFound("User not found.");

            var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();
            if (config == null)
                return NotFound("Overtime configuration not found.");

            // Compute total worked hours
            TimeSpan breakDuration = TimeSpan.Zero;

            if (attendance.BreakFinish.HasValue && attendance.BreakStart.HasValue)
            {
                DateTime breakStart = attendance.BreakFinish.Value; // Switch order
                DateTime breakEnd = attendance.BreakStart.Value; // Switch order

                // Validate that break time is within the shift
                if (breakStart >= attendance.ClockIn && breakEnd <= attendance.ClockOut)
                {
                    breakDuration = breakEnd - breakStart;
                }
            }

            TimeSpan totalWorkDuration = (attendance.ClockOut.Value - attendance.ClockIn) - breakDuration;
            double totalWorkHours = totalWorkDuration.TotalHours;

            // Compute regular work hours
            double breakTimeHours = config.BreaktimeMax / 60.0; // Convert minutes to hours
            double regularWorkHours = (config.OfficeEndTime - config.OfficeStartTime).TotalHours - breakTimeHours;

            // Check for approved overtime request
            var approvedOvertime = await _context.Overtimes
                .Where(o => o.UserId == user.Id
                    && o.Date.Date == attendance.ClockIn.Date
                    && o.Status == OvertimeRequestStatus.Approved)
                .FirstOrDefaultAsync();

            double overtimeHours = 0;

            if (totalWorkHours > regularWorkHours && approvedOvertime != null)
            {
                // Calculate the approved overtime duration
                double approvedOvertimeDuration = (approvedOvertime.EndTime - approvedOvertime.StartTime).TotalHours;

                // Compute the actual overtime worked (total work hours - regular work hours)
                double actualOvertimeWorked = totalWorkHours - regularWorkHours;

                overtimeHours = Math.Min(actualOvertimeWorked, Math.Min(approvedOvertimeDuration, config.OvertimeDailyMax));

                // Add to user's accumulated overtime
                user.AccumulatedOvertime += overtimeHours;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Clock-out successfully.",
                totalWorkHours = $"{totalWorkHours:F2}h",
                approvedOvertime = approvedOvertime != null,
                approvedOvertimeDuration = approvedOvertime != null ? $"{approvedOvertime.EndTime.Subtract(approvedOvertime.StartTime).TotalHours:F2}h" : "0h",
                actualOvertimeWorked = $"{(totalWorkHours - regularWorkHours):F2}h",
                overtimeAdded = $"{overtimeHours:F2}h",
                newAccumulatedOvertime = $"{Math.Floor(user.AccumulatedOvertime)}h {Math.Round((user.AccumulatedOvertime % 1) * 60)}m"
            });

        }

        [HttpPut("{id}/start-break")]
        [Authorize]
        public async Task<IActionResult> StartBreak(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
                return NotFound("Attendance not found.");

            if (attendance.ClockOut.HasValue)
                return BadRequest("Cannot start break because you are already clocked out.");

            if (attendance.BreakStart.HasValue || attendance.BreakFinish.HasValue)
                return BadRequest("Break has already been started or ended.");

            attendance.BreakStart = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Break has started." });
        }

        [HttpPut("{id}/end-break")]
        [Authorize]
        public async Task<IActionResult> EndBreak(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
                return NotFound("Attendance not found.");

            if (!attendance.BreakStart.HasValue)
                return BadRequest("Cannot end break because break has not been started.");

            // Check if the user has already clocked out
            if (attendance.ClockOut.HasValue)
                return BadRequest("Cannot end break because you are already clocked out.");

            // Optionally, check if the break has already ended to prevent multiple submissions.
            if (attendance.BreakFinish.HasValue)
                return BadRequest("Break has already been ended.");

            attendance.BreakFinish = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Break has ended.", breakDuration = attendance.FormattedBreakDuration });
        }

        [HttpPut("{id}/edit-attendance")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditAttendanceRecord(int id, [FromBody] EditAttendanceRecordDto updatedAttendance)
        {
            var attendance = await _context.Attendances.FindAsync(id);

            if(attendance == null)
            {
                return BadRequest("Attendance Record not Found");
            }

            attendance.ClockIn = updatedAttendance.ClockIn;
            attendance.ClockOut = updatedAttendance.ClockOut;

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

            response.Add("totalWorkHours", attendance.FormattedBreakDuration);
            response.Add("totalWorkHours", attendance.FormattedWorkDuration);

            return Ok(response);
        }
    }
}
