using AttendanceTracker1.DTO;

namespace AttendanceTracker1.Services.LogService
{
    public interface ILogService
    {
        public Task<object> GetLogs(int page, int pageSize);
    }
}
