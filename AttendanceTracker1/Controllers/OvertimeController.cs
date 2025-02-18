using System.Security.Claims;
using System.Text.RegularExpressions;
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
        public async Task<IActionResult> GetOvertimeRequests(int page = 1, int pageSize = 10)
        {
            var skip = (page - 1) * pageSize;

            var totalRecords = await _context.Overtimes.CountAsync();

            // Fetch the overtime requests with pagination
            var overtimes = await _context.Overtimes
                .Include(o => o.User)
                .Include(o => o.Approver)
                .OrderBy(o => o.CreatedAt) // Stable ordering
                .Skip(skip) // Skip the records for the previous pages
                .Take(pageSize) // Limit the number of records to the page size
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
                    ApproverName = o.Approver != null ? o.Approver.Name : null,
                    RejectionReason = o.RejectionReason,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .ToListAsync();

            // Calculate total pages
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Return the paginated results along with metadata
            return Ok(new
            {
                data = overtimes,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            });
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
                    ApproverName = o.Approver != null ? o.Approver.Name : null,
                    RejectionReason = o.RejectionReason,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (overtime == null)
            {
                return NotFound("Overtime request not found.");
            }

            return Ok(overtime);
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetOvertimeRequestByUserId(int id)
        {
            var overtime = await _context.Overtimes
                .Where(o => o.UserId == id)
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
                    ApproverName = o.Approver != null ? o.Approver.Name : null,
                    RejectionReason = o.RejectionReason,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
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

            // Get the user ID from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Invalid token.");
            }

            var userId = int.Parse(userIdClaim); // Convert string to integer if necessary

            // 🔹 Create Overtime Request
            var overtime = new Overtime
            {
                UserId = userId,
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

            // Check if RejectionReason is provided when status is Rejected
            if (request.Status == OvertimeRequestStatus.Rejected &&
                string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                return BadRequest("Rejection reason is required when status is Rejected.");
            }

            // Get the user ID from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("Invalid token.");
            }

            var userId = int.Parse(userIdClaim); // Convert string to integer if necessary

            overtime.Status = request.Status;
            overtime.ReviewedBy = userId;
            overtime.RejectionReason = request.RejectionReason;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Overtime request {id} has been {overtime.Status}." });
        }
    }
}
