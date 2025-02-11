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
    public class LeaveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaveController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLeaveRequests()
        {
            var leaves = await _context.Leaves
                .Include(l => l.User)
                .Include(l => l.Approver)
                .Select(l => new LeaveResponseDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User != null ? l.User.Name : null, // Avoids cyclic reference
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    DaysCount = l.DaysCount,
                    StatusName = l.Status.ToString(),  
                    TypeName = l.Type.ToString(),    
                    Reason = l.Reason,
                    ReviewedBy = l.ReviewedBy,
                    ApproverName = l.Approver != null ? l.Approver.Name : null,
                    CreatedDate = l.CreatedDate
                })
                .ToListAsync();

            return Ok(leaves);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetLeaveRequestById(int id)
        {
            var leave = await _context.Leaves
                .Where(l => l.Id == id)
                .Include(l => l.User)
                .Include(l => l.Approver)
                .Select(l => new LeaveResponseDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User != null ? l.User.Name : null, // Avoids cyclic reference
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    DaysCount = l.DaysCount,
                    StatusName = l.Status.ToString(),
                    TypeName = l.Type.ToString(),
                    Reason = l.Reason,
                    ReviewedBy = l.ReviewedBy,
                    ApproverName = l.Approver != null ? l.Approver.Name : null,
                    CreatedDate = l.CreatedDate
                })
                .FirstOrDefaultAsync();

            return Ok(leave);
        }

        [HttpGet("userId")]
        [Authorize]
        public async Task<IActionResult> GetLeaveRequestByUserId(int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var leave = await _context.Leaves
                .Where(l => l.UserId == userId)
                .Include(l => l.User)
                .Include(l => l.Approver)
                .Select(l => new LeaveResponseDto
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    UserName = l.User != null ? l.User.Name : null, // Avoids cyclic reference
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    DaysCount = l.DaysCount,
                    StatusName = l.Status.ToString(),
                    TypeName = l.Type.ToString(),
                    Reason = l.Reason,
                    ReviewedBy = l.ReviewedBy,
                    ApproverName = l.Approver != null ? l.Approver.Name : null,
                    CreatedDate = l.CreatedDate
                })
                .ToListAsync();

            if (!leave.Any())
            {
                return NotFound($"User with ID {userId} has no leave requests.");
            }

            return Ok(leave);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestLeave([FromBody] RequestLeaveDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var leaveRequest = new Leave
            {
                UserId = request.UserId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Reason = request.Reason,
                Type = request.Type,
                Status = LeaveStatus.Pending, // Default to Pending until reviewed
                ReviewedBy = request.RequiresApproval ? null : request.ReviewedBy, // Null if approval is required
                CreatedDate = DateTime.UtcNow
            };

            _context.Leaves.Add(leaveRequest);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Leave request submitted successfully!",
                leaveId = leaveRequest.Id,
                status = leaveRequest.Status.ToString()
            });
        }

        [HttpPut("{id}/review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Review (int id, [FromBody] LeaveReviewDto request)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null)
            {
                return NotFound($"Request with leave id: {id} was not found.");
            }

            // ✅ Validate if status is a valid enum value
            if (!Enum.IsDefined(typeof(LeaveStatus), request.Status))
            {
                return BadRequest("Invalid leave status.");
            }

            leave.Status = request.Status;
            leave.ReviewedBy = request?.ReviewedBy;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Leave request {id} has been {leave.Status}." });
        }
    }
}
