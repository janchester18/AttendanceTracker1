using AttendanceTracker1.Data;
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
                .ToListAsync();

            return Ok(attendances);
        }

        [HttpGet("userId")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceByUser(int userId)
        {
            var attendances = await _context.Attendances
                .Where(a => a.UserId == userId)
                .Include(a => a.User)
                .ToListAsync();

            return Ok(attendances);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ClockIn([FromBody] Attendance attendance)
        {
            if (attendance == null) return BadRequest("Invalid data.");

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAttendanceByUser), new { userId = attendance.UserId }, attendance);
        }

        [HttpPut("{id}/clockout")]
        [Authorize]
        public async Task<IActionResult> ClockOut(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return NotFound("Attendance not found.");

            attendance.ClockOut = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(attendance);
        }
    }
}
