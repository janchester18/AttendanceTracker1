using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;

namespace AttendanceTracker1.Services.LogService
{
    public interface ILogService
    {
        public Task<ApiResponse<object>> GetLogs(int page, int pageSize);
    }
}
