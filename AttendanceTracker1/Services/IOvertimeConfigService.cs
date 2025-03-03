using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Services
{
    public interface IOvertimeConfigService
    {
        public Task<ApiResponse<object>> UpdateConfig(OvertimeConfigDto updatedConfig);
        public Task<ApiResponse<object>> GetOvertimeConfig();
    }
}
