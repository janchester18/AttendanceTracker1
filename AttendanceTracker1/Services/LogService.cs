using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<LogResponseDto>> GetLogs(int page, int pageSize)
        {
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

            return logs;
        }
    }
}
