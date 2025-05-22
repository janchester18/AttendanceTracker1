using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services.NotificationService;
using DENR_IHRMIS.Data;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Security.Claims;

namespace AttendanceTracker1.Services.LeaveService
{
    public class LeaveService : ILeaveService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;
        public LeaveService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, INotificationService notificationService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
        }
        public async Task<ApiResponse<object>> GetLeaveRequests(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var totalRecords = await _context.Leaves.CountAsync();

            var leaves = await _context.Leaves
                .Include(l => l.User)
                .Include(l => l.Approver)
                .OrderByDescending(l => l.CreatedDate) // Stable ordering
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

            if (leave == null) 
                return ApiResponse<object>.Success(null, "Leave record not found.");

            return ApiResponse<object>.Success(leave, "Leave record requested successfully.");
        }
        public async Task<ApiResponse<object>> GetLeaveRequestByUserId(int id)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!userExists) 
                return ApiResponse<object>.Success(null, $"User with ID {id} not found.");

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

            if (!leave.Any()) 
                return ApiResponse<object>.Success(null, $"User with ID {id} has no leave requests.");

            return ApiResponse<object>.Success(leave, "Leave record requested successfully");
        }

        public async Task<ApiResponse<object>> GetSelfLeaveRequest(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(userIdClaim);

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return ApiResponse<object>.Success(null, $"User with ID {userId} not found.");

            var leave = await _context.Leaves
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedDate) // Stable ordering
                .Skip(skip) // Skip the records for the previous pages
                .Take(pageSize) // Limit the number of records to the page size
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
                    Type = l.Type,
                    TypeName = l.Type.ToString(),
                    Reason = l.Reason,
                    ReviewedBy = l.ReviewedBy,
                    ApproverName = l.Approver != null ? l.Approver.Name : null,
                    RejectionReason = l.RejectionReason,
                    CreatedDate = l.CreatedDate
                })
                .ToListAsync();

            if (!leave.Any())
                return ApiResponse<object>.Success(null, $"User with ID {userId} has no leave requests.");

            var totalRecords = await _context.Leaves
                .Where(l => l.UserId == userId) // Filter by userId
                .CountAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return ApiResponse<object>.Success(new
            {
                leave,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "Leave data request successful.");
        }

        public async Task<ApiResponse<object>> RequestLeave(RequestLeaveDto request)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(userIdClaim);

            if (request.EndDate < request.StartDate)
                return ApiResponse<object>.Success(null, "Start date cannot be before end date.");


            var leaveRequest = new Leave
            {
                UserId = userId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Reason = request.Reason,
                Type = request.Type,
                CreatedDate = DateTimeHelper.ConvertToPST(DateTime.UtcNow)
            };

            _context.Leaves.Add(leaveRequest);
            await _context.SaveChangesAsync();

            var response = ApiResponse<object>.Success(new
            {
                leaveId = leaveRequest.Id,
                status = leaveRequest.Status.ToString()
            }, "Leave request submitted successfully!");

            var notificationMessage = $"{username} has requested a leave from {request.StartDate:MMM dd, yyyy} to {request.EndDate:MMM dd, yyyy}.";

            var notification = await _notificationService.CreateAdminNotification(
                title: "New Leave Request",
                message: notificationMessage,
                link: "/api/notification/view/{id}", 
                createdById: userId,
                type: "Leave Request"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Leave")
                .Information("{UserName} has requested a leave at {Time}", username, DateTimeHelper.ConvertToPST(DateTime.UtcNow));

            return response;
        }
        public async Task<ApiResponse<object>> Review(int id, LeaveReviewDto request)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null) 
                return ApiResponse<object>.Success(null, $"Request with leave id: {id} was not found.");

            // ✅ Validate if status is a valid enum value
            if (!Enum.IsDefined(typeof(LeaveStatus), request.Status)) 
                return ApiResponse<object>.Success(null, "Invalid leave status.");

            // Check if RejectionReason is provided when status is Rejected
            if (request.Status == LeaveStatus.Rejected &&
                string.IsNullOrWhiteSpace(request.RejectionReason)) 
                return ApiResponse<object>.Success(null, "Rejection reason is required when status is Rejected.");

            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminUsername = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminIdClaim)) 
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(adminIdClaim);

            leave.Status = request.Status;
            leave.ReviewedBy = userId;
            leave.RejectionReason = request.RejectionReason;

            await _context.SaveChangesAsync();

            var action = leave.Status.ToString();

            var notificationMessage = $"{adminUsername} has {action} your leave from {leave.StartDate:MMM dd, yyyy} to {leave.EndDate:MMM dd, yyyy}."; //ADD ADMIN NOTIFICATION

            var notification = await _notificationService.CreateNotification(
                userId: leave.UserId,
                title: "Leave Review Result",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                type: "Leave Review"
            );
            
            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Leave")
                .Information("{UserName} has {Action} leave {Id} at {Time}", adminUsername, action, id, DateTimeHelper.ConvertToPST(DateTime.UtcNow));

            return ApiResponse<object>.Success(null, $"Leave request {id} has been {leave.Status}.");
        }

        public async Task<ApiResponse<object>> Approve(int id)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null)
                return ApiResponse<object>.Success(null, $"Request with leave id: {id} was not found.");

            if (leave.Status != LeaveStatus.Pending)
                return ApiResponse<object>.Success(null, "Can not review a request that is not pending.");

            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminUsername = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(adminIdClaim);

            leave.Status = LeaveStatus.Approved;
            leave.ReviewedBy = userId;

            await _context.SaveChangesAsync();

            var action = leave.Status.ToString();

            var notificationMessage = $"{adminUsername} has {action} your leave from {leave.StartDate:MMM dd, yyyy} to {leave.EndDate:MMM dd, yyyy}."; //ADD ADMIN NOTIFICATION

            var notification = await _notificationService.CreateNotification(
                userId: leave.UserId,
                title: "Leave Review Result",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                type: "Leave Review"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Leave")
                .Information("{UserName} has {Action} leave {Id} at {Time}", adminUsername, action, id, DateTimeHelper.ConvertToPST(DateTime.UtcNow));

            return ApiResponse<object>.Success(null, $"Leave request {id} has been {leave.Status}.");
        }

        public async Task<ApiResponse<object>> Reject(int id, LeaveRejectDto request)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null)
                return ApiResponse<object>.Success(null, $"Request with leave id: {id} was not found.");

            if (leave.Status != LeaveStatus.Pending)
                return ApiResponse<object>.Success(null, "Can not review a request that is not pending.");

            // Check if RejectionReason is provided when status is Rejected
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
                return ApiResponse<object>.Success(null, "Rejection reason is required when status is Rejected.");

            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminUsername = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(adminIdClaim);

            leave.Status = LeaveStatus.Rejected;
            leave.ReviewedBy = userId;
            leave.RejectionReason = request.RejectionReason;

            await _context.SaveChangesAsync();

            var action = leave.Status.ToString();

            var notificationMessage = $"{adminUsername} has {action} your leave from {leave.StartDate:MMM dd, yyyy} to {leave.EndDate:MMM dd, yyyy}."; //ADD ADMIN NOTIFICATION

            var notification = await _notificationService.CreateNotification(
                userId: leave.UserId,
                title: "Leave Review Result",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                type: "Leave Review"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Leave")
                .Information("{UserName} has {Action} leave {Id} at {Time}", adminUsername, action, id, DateTimeHelper.ConvertToPST(DateTime.UtcNow));

            return ApiResponse<object>.Success(null, $"Leave request {id} has been {leave.Status}.");
        }

        public async Task<ApiResponse<object>> UpdateLeaveRequest(int id, UpdateLeaveDto request)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null)
                return ApiResponse<object>.Success(null, $"Leave request with ID {id} not found.");

            if (leave.Status != LeaveStatus.Pending)
                return ApiResponse<object>.Success(null, "You can only update pending leave requests.");

            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(userIdClaim);

            if (leave.UserId != userId)
                return ApiResponse<object>.Success(null, "You can only update your own leave requests.");

            leave.StartDate = request.StartDate ?? leave.StartDate;
            leave.EndDate = request.EndDate ?? leave.EndDate;
            leave.Reason = request.Reason ?? leave.Reason;
            leave.Type = request.Type ?? leave.Type;
            leave.UpdatedAt = DateTimeHelper.ConvertToPST(DateTime.UtcNow);

            await _context.SaveChangesAsync();

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Leave")
                .Information("{UserName} has updated leave request {Id} at {Time}", username, id, DateTimeHelper.ConvertToPST(DateTime.UtcNow));

            return ApiResponse<object>.Success(leave, "Leave request updated successfully.");
        }

        public async Task<ApiResponse<object>> CancelLeaveRequest(int id)
        {
            var leave = await _context.Leaves.FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null)
                return ApiResponse<object>.Success(null, $"Leave request with ID {id} not found.");

            if (leave.Status != LeaveStatus.Pending)
                return ApiResponse<object>.Success(null, "You can only cancel pending leave requests.");

            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;


            if (string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(userIdClaim);

            if (leave.UserId != userId)
                return ApiResponse<object>.Success(null, "You can only cancel your own leave requests.");


            leave.Status = LeaveStatus.Canceled;

            _context.Leaves.Update(leave);
            await _context.SaveChangesAsync();

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Leave")
                .Information("{UserName} has canceled leave request {Id} at {Time}", username, id, DateTimeHelper.ConvertToPST(DateTime.UtcNow));

            return ApiResponse<object>.Success(leave, "Leave request canceled successfully.");
        }

    }
}
