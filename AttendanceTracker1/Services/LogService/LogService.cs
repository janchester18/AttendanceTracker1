using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.EntityFrameworkCore;
using static AttendanceTracker1.Models.User;

namespace AttendanceTracker1.Services.LogService
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse<object>> GetLogs(int page, int pageSize)
        {
            var today = DateTime.UtcNow.Date; // Ensure you're using the correct timezone

            // Count logs excluding "CashAdvance"
            var totalRecords = await _context.Logs
                .Where(l => l.Type != "CashAdvance")
                .CountAsync();

            if (totalRecords == 0)
                return ApiResponse<object>.Success(new { Data = new List<LogResponseDto>() }, "No logs found.");

            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Fetch paginated logs
            var logs = await _context.Logs
                .Where(l => l.Type != "CashAdvance")
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LogResponseDto
                {
                    Id = l.Id,
                    Message = l.Message,
                    Timestamp = l.Timestamp,
                    Type = l.Type
                })
                .ToListAsync();

            // Additional Counts
            var totalUsers = await _context.Users.Where(u => u.SystemUserType == "Attendance" && u.VisibilityStatus == UserVisibilityStatus.Enabled).CountAsync();
            var attendanceToday = await _context.Attendances
                .Where(a => a.Date == today)
                .CountAsync();
            var approvedLeavesToday = await _context.Leaves
                .Where(lr => lr.Status == Models.LeaveStatus.Approved &&
                             lr.StartDate <= today && lr.EndDate >= today)
                .CountAsync();

            var response = new
            {
                Data = logs,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1,

                // Additional Data
                TotalUsers = totalUsers,
                AttendanceToday = attendanceToday,
                ApprovedLeavesToday = approvedLeavesToday
            };

            return ApiResponse<object>.Success(response, "Logs retrieved successfully.");
        }


    }
}
