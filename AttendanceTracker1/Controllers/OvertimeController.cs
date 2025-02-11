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
    public class OvertimeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public OvertimeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOvertimeRequests ()
        {
            var overtimes = await _context.Overtimes
                .Include(o => o.User)
                .Include(o => o.Approver)
                .Select(o => new OvertimeResponseDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    EmployeeName = o.User != null ? o.User.Name : "Unknown",
                    Date = o.Date,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    Reason = o.Reason,
                    Status = o.Status.ToString(),
                    ReviewedBy = o.ReviewedBy,
                    ApproverName = o.Approver != null ? o.Approver.Name : null
                })
                .ToListAsync();

            return Ok(overtimes);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOvertimeRequestById(int id)
        {
            var overtime = await _context.Overtimes
                .Where(o => o.Id == id)
                .Include(o => o.User)
                .Include(o => o.Approver)
                .Select(o => new OvertimeResponseDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    EmployeeName = o.User != null ? o.User.Name : "Unknown",
                    Date = o.Date,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    Reason = o.Reason,
                    Status = o.Status.ToString(),
                    ReviewedBy = o.ReviewedBy,
                    ApproverName = o.Approver != null ? o.Approver.Name : null
                })
                .FirstOrDefaultAsync();

            if (overtime == null)
            {
                return NotFound("Overtime request not found.");
            }

            return Ok(overtime);
        }

        [HttpGet("userId")]
        [Authorize]
        public async Task<IActionResult> GetOvertimeRequestByUserId(int userId)
        {
            var overtime = await _context.Overtimes
                .Where(o => o.UserId == userId)
                .Include(o => o.User)
                .Include(o => o.Approver)
                .Select(o => new OvertimeResponseDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    EmployeeName = o.User != null ? o.User.Name : "Unknown",
                    Date = o.Date,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    Reason = o.Reason,
                    Status = o.Status.ToString(),
                    ReviewedBy = o.ReviewedBy,
                    ApproverName = o.Approver != null ? o.Approver.Name : null
                })
                .ToListAsync();

            return Ok(overtime);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestOvertime([FromBody] OvertimeRequestDto overtimeRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (overtimeRequest.StartTime >= overtimeRequest.EndTime)
            {
                return BadRequest("Start time must be before end time.");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == overtimeRequest.UserId);
            if (!userExists)
            {
                return NotFound("User not found.");
            }

            // 🔹 Create Overtime Request
            var overtime = new Overtime
            {
                UserId = overtimeRequest.UserId,
                Date = overtimeRequest.Date,
                StartTime = overtimeRequest.StartTime,
                EndTime = overtimeRequest.EndTime,
                Reason = overtimeRequest.Reason,
                Status = OvertimeRequestStatus.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Overtimes.Add(overtime);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Overtime request submitted successfully.", OvertimeId = overtime.Id });
        }

        [HttpPut("{id}/review")]
        [Authorize]
        public async Task<IActionResult> Review(int id, [FromBody] OvertimeReview request)
        {
            var overtime = await _context.Overtimes.FirstOrDefaultAsync(o => o.Id == id);
            if (overtime == null)
            {
                return NotFound("User not found.");
            }

            // ✅ Validate if status is a valid enum value
            if (!Enum.IsDefined(typeof(OvertimeRequestStatus), request.Status))
            {
                return BadRequest("Invalid leave status.");
            }

            overtime.Status = request.Status;
            overtime.ReviewedBy = request.ReviewedBy;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Overtime request {id} has been {overtime.Status}." });
        }
    }
}
