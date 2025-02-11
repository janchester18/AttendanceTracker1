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

            
            if (!Enum.IsDefined(typeof(AttendanceStatus), clockInDto.Status))
            {
                return BadRequest(new { error = "Invalid status value." });
            }
            AttendanceStatus status = (AttendanceStatus)clockInDto.Status;

            
            var attendance = new Attendance
            {
                UserId = clockInDto.UserId,
                Date = today,
                ClockIn = DateTime.Now,
                ClockOut = parsedClockOut,
                Status = status,
                Remarks = clockInDto.Remarks,
                ClockInLatitude = clockInDto.ClockInLatitude,
                ClockInLongitude = clockInDto.ClockInLongitude
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok(new 
            { message = "Clock-in recorded successfully.", 
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

            attendance.ClockOut = DateTime.Now;
            attendance.ClockOutLatitude = clockOutDto.ClockOutLatitude;
            attendance.ClockOutLongitude = clockOutDto.ClockOutLongitude;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Clock-out successfully.",
                totalWorkHours = attendance.FormattedWorkDuration,
                clockoutLatitude = clockOutDto.ClockOutLatitude,
                clockoutLongitude = clockOutDto.ClockOutLongitude
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
