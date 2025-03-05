using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Security.Claims;

namespace AttendanceTracker1.Services
{
    public class OvertimeService : IOvertimeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;
        public OvertimeService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, INotificationService notificationService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
        }

        //GET OVERTIME REQUESTS SERVICE
        public async Task<ApiResponse<object>> GetOvertimeRequests(int page, int pageSize)
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
                    ExpectedOutput = o.ExpectedOutput,
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

            return ApiResponse<object>.Success(new
            {
                overtimes,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "Overtime data request successful.");
        }

        //GET OVERTIME REQUEST BY ID SERVICE
        public async Task<ApiResponse<object>> GetOvertimeRequestById(int id)
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
                   ExpectedOutput = o.ExpectedOutput,
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
                return (ApiResponse<object>.Success(null, "Overtime request not found."));
            }

            return (ApiResponse<object>.Success(overtime, "Overtime data request successful."));
        }

        //GET OVERTIME REQUEST BY USER ID SERVICE
        public async Task<ApiResponse<object>> GetOvertimeRequestByUserId(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return (ApiResponse<object>.Success(null, "User not found."));
            }

            var overtime = await _context.Overtimes
            .Where(o => o.UserId == id)
            .Include(o => o.User)
            .Include(o => o.Approver)
            .Select(o => new OvertimeResponseDto
            {
                Id = o.Id,
                EmployeeName = o.User != null ? o.User.Name : "Unknown",
                Date = o.Date,
                StartTime = o.StartTime,
                EndTime = o.EndTime,
                Reason = o.Reason,
                ExpectedOutput = o.ExpectedOutput,
                Status = o.Status.ToString(),
                ReviewedBy = o.ReviewedBy,
                ApproverName = o.Approver != null ? o.Approver.Name : null,
                RejectionReason = o.RejectionReason,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            })
            .ToListAsync();

            return (ApiResponse<object>.Success(overtime, "Overtime data request successful."));
        }

        // REQUEST OVERTIME SERVICE
        public async Task<ApiResponse<object>> RequestOvertime(OvertimeRequestDto overtimeRequest)
        {
            if (overtimeRequest.StartTime >= overtimeRequest.EndTime)
            {
                return (ApiResponse<object>.Success(null, "Start time must be before end time."));
            }

            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
            {
                return (ApiResponse<object>.Success(null, "Invalid token."));
            }

            var userId = int.Parse(userIdClaim);

            // 🔹 Create Overtime Request
            var overtime = new Overtime
            {
                UserId = userId,
                Date = overtimeRequest.Date,
                StartTime = overtimeRequest.StartTime,
                EndTime = overtimeRequest.EndTime,
                Reason = overtimeRequest.Reason,
                ExpectedOutput = overtimeRequest.ExpectedOutput,
                Status = OvertimeRequestStatus.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Overtimes.Add(overtime);
            await _context.SaveChangesAsync();

            var notificationMessage = $"{username} has requested an overtime on {overtimeRequest.Date:MMM dd, yyyy} from {overtimeRequest.StartTime} to {overtimeRequest.EndTime}.";

            var notification = await _notificationService.CreateAdminNotification(
                title: "New Overtime Request",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                type: "Overtime Request"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Overtime")
                .Information("{UserName} has requested an overtime at {Time}", username, DateTime.Now);

            return (ApiResponse<object>.Success(new { OvertimeId = overtime.Id }, "Overtime request submitted successfully."));
        }

        //OVERTIME REVIEW SERVICE
        public async Task<ApiResponse<object>> Review(int id, OvertimeReview request)
        {
            var overtime = await _context.Overtimes.FirstOrDefaultAsync(o => o.Id == id);
            if (overtime == null) return (ApiResponse<object>.Success(null, "User not found."));

            // ✅ Validate if status is a valid enum value
            if (!Enum.IsDefined(typeof(OvertimeRequestStatus), request.Status)) return (ApiResponse<object>.Success(null, "Invalid leave status."));

            // Check if RejectionReason is provided when status is Rejected
            if (request.Status == OvertimeRequestStatus.Rejected &&
                string.IsNullOrWhiteSpace(request.RejectionReason)) return (ApiResponse<object>.Success(null, "Rejection reason is required when status is Rejected."));

            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminUsername = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminIdClaim))
                return (ApiResponse<object>.Success(null, "Invalid token."));

            var userId = int.Parse(adminIdClaim);

            overtime.Status = request.Status;
            overtime.ReviewedBy = userId;
            overtime.RejectionReason = request.RejectionReason;

            await _context.SaveChangesAsync();

            var action = overtime.Status.ToString();
            
            var notificationMessage = $"{adminUsername} has {action} your overtime on {overtime.Date:MMM dd, yyyy} from {overtime.StartTime} to {overtime.EndTime}.";
            
            var notification = await _notificationService.CreateNotification(
                userId: overtime.UserId,
                title: "Overtime Review Result",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                type: "Overtime Review"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Overtime")
                .Information("{UserName} has {Action} overtime {Id} at {Time}", adminUsername, action, id, DateTime.Now);

            return (ApiResponse<object>.Success(null, $"Overtime request {id} has been {overtime.Status}."));
        }
    }
}
