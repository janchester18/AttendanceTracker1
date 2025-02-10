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
                    Status = l.Status,
                    Type = l.Type,
                    Reason = l.Reason,
                    ReviewedBy = l.ReviewedBy,
                    ApproverName = l.Approver != null ? l.Approver.Name : null,
                    CreatedDate = l.CreatedDate
                })
                .ToListAsync();

            return Ok(leaves);
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


    }
}
