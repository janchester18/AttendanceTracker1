using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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
        public async Task<IActionResult> GetLeaveRequests(int page = 1, int pageSize = 10)
        {
            try
            {
                var skip = (page - 1) * pageSize;

                var totalRecords = await _context.Leaves.CountAsync();

                var leaves = await _context.Leaves
                    .Include(l => l.User)
                    .Include(l => l.Approver)
                    .OrderBy(l => l.CreatedDate) // Stable ordering
                    .Skip(skip) // Skip the records for the previous pages
                    .Take(pageSize) // Limit the number of records to the page size
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
                        RejectionReason = l.RejectionReason,
                        CreatedDate = l.CreatedDate
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var response = ApiResponse<object>.Success(new
                {
                    leaves = leaves,
                    totalRecords,
                    totalPages,
                    currentPage = page,
                    pageSize,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                }, "Leave data request successful.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetLeaveRequestById(int id)
        {
            try
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
                    RejectionReason = l.RejectionReason,
                    CreatedDate = l.CreatedDate
                })
                .FirstOrDefaultAsync(); 

                if(leave == null) return Ok(ApiResponse<object>.Success(null, "Leave record not found."));

                return Ok(ApiResponse<object>.Success(leave, "Leave record requested successfully."));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetLeaveRequestByUserId(int id)
        {
            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == id);
                if (!userExists) return Ok(ApiResponse<object>.Success(null, $"User with ID {id} not found."));

                var leave = await _context.Leaves
                    .Where(l => l.UserId == id)
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
                        RejectionReason = l.RejectionReason,
                        CreatedDate = l.CreatedDate
                    })
                    .ToListAsync();

                if (!leave.Any()) return Ok(ApiResponse<object>.Success(null, $"User with ID {id} has no leave requests."));

                return Ok(ApiResponse<object>.Success(leave, "Leave record requested successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestLeave([FromBody] RequestLeaveDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Ok(ApiResponse<object>.Success(null, "Invalid token."));
                }

                var userId = int.Parse(userIdClaim);

                var leaveRequest = new Leave
                {
                    UserId = userId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Reason = request.Reason,
                    Type = request.Type,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Leaves.Add(leaveRequest);
                await _context.SaveChangesAsync();

                var response = ApiResponse<object>.Success(new
                {
                    leaveId = leaveRequest.Id,
                    status = leaveRequest.Status.ToString()
                }, "Leave request submitted successfully!");

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Leave")
                    .Information("{UserName} has requested a leave at {Time}", username, DateTime.Now);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }

        [HttpPut("review/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Review (int id, [FromBody] LeaveReviewDto request)
        {
            try
            {
                var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
                if (leave == null) return Ok(ApiResponse<object>.Success(null, $"Request with leave id: {id} was not found."));
                
                // ✅ Validate if status is a valid enum value
                if (!Enum.IsDefined(typeof(LeaveStatus), request.Status)) return Ok(ApiResponse<object>.Success(null, "Invalid leave status."));
                
                // Check if RejectionReason is provided when status is Rejected
                if (request.Status == LeaveStatus.Rejected &&
                    string.IsNullOrWhiteSpace(request.RejectionReason)) return Ok(ApiResponse<object>.Success(null, "Rejection reason is required when status is Rejected."));

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim)) return Ok(ApiResponse<object>.Success(null, "Invalid token."));

                var userId = int.Parse(userIdClaim);

                leave.Status = request.Status;
                leave.ReviewedBy = userId;
                leave.RejectionReason = request.RejectionReason;

                await _context.SaveChangesAsync();

                var action = leave.Status.ToString();

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Leave")
                    .Information("{UserName} has {Action} leave {Id} at {Time}", username, action, id, DateTime.Now);

                return Ok(ApiResponse<object>.Success(null,  $"Leave request {id} has been {leave.Status}."));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }
    }
}
