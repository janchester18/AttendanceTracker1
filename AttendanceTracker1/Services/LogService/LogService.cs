using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Services.LogService
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<object> GetLogs(int page, int pageSize)
        {
            var totalRecords = await _context.Logs.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var today = DateTime.UtcNow.Date; // Ensure you're using the correct timezone

            var logs = await _context.Logs
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
            var totalUsers = await _context.Users.CountAsync();
            var attendanceToday = await _context.Attendances
                .Where(a => a.Date == today)
                .CountAsync();
            var approvedLeavesToday = await _context.Leaves
                 .Where(lr => lr.Status == Models.LeaveStatus.Approved &&
                 lr.StartDate <= today && lr.EndDate >= today)
                .CountAsync();

            return new
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
        }

    }
}
