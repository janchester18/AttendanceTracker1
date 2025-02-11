using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
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
    }
}
