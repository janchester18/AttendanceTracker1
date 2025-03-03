using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AttendanceTracker1.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LeaveService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ApiResponse<object>> GetLeaveRequests(int page, int pageSize)
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

            return ApiResponse<object>.Success(new
            {
                leaves,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "Leave data request successful.");
        }
        public async Task<ApiResponse<object>> GetLeaveRequestById(int id)
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

            if (leave == null) return (ApiResponse<object>.Success(null, "Leave record not found."));

            return (ApiResponse<object>.Success(leave, "Leave record requested successfully."));
        }
        public async Task<ApiResponse<object>> GetLeaveRequestByUserId(int id)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!userExists) return (ApiResponse<object>.Success(null, $"User with ID {id} not found."));

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

            if (!leave.Any()) return (ApiResponse<object>.Success(null, $"User with ID {id} has no leave requests."));

            return (ApiResponse<object>.Success(leave, "Leave record requested successfully"));
        }
        public async Task<ApiResponse<object>> RequestLeave(RequestLeaveDto request)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
            {
                return (ApiResponse<object>.Success(null, "Invalid token."));
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

            return (response);
        }
        public async Task<ApiResponse<object>> Review(int id, LeaveReviewDto request)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null) return (ApiResponse<object>.Success(null, $"Request with leave id: {id} was not found."));

            // ✅ Validate if status is a valid enum value
            if (!Enum.IsDefined(typeof(LeaveStatus), request.Status)) return (ApiResponse<object>.Success(null, "Invalid leave status."));

            // Check if RejectionReason is provided when status is Rejected
            if (request.Status == LeaveStatus.Rejected &&
                string.IsNullOrWhiteSpace(request.RejectionReason)) return (ApiResponse<object>.Success(null, "Rejection reason is required when status is Rejected."));

            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminUsername = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminIdClaim)) return (ApiResponse<object>.Success(null, "Invalid token."));

            var userId = int.Parse(adminIdClaim);

            leave.Status = request.Status;
            leave.ReviewedBy = userId;
            leave.RejectionReason = request.RejectionReason;

            await _context.SaveChangesAsync();

            var action = leave.Status.ToString();

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Leave")
                .Information("{UserName} has {Action} leave {Id} at {Time}", adminUsername, action, id, DateTime.Now);

            return (ApiResponse<object>.Success(null, $"Leave request {id} has been {leave.Status}."));
        }
    }
}
