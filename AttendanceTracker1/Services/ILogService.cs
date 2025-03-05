using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;

namespace AttendanceTracker1.Services
{
    public interface ILogService
    {
        public Task<IEnumerable<LogResponseDto>> GetLogs(int page, int pageSize);
    }
}
