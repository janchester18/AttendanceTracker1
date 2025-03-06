using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;

namespace AttendanceTracker1.Services
{
    public interface IOvertimeConfigService
    {
        public Task<ApiResponse<object>> UpdateConfig(OvertimeConfigDto updatedConfig);
        public Task<ApiResponse<object>> GetOvertimeConfig();
    }
}
