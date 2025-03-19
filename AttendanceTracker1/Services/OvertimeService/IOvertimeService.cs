using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;

namespace AttendanceTracker1.Services.OvertimeService
{
    public interface IOvertimeService
    {
        public Task<ApiResponse<object>> GetOvertimeRequests(int page, int pageSize);
        public Task<ApiResponse<object>> GetOvertimeRequestById(int id);
        public Task<ApiResponse<object>> GetOvertimeRequestByUserId(int id);
        public Task<ApiResponse<object>> GetSelfOvertimeRequest(int page, int pageSize);
        public Task<ApiResponse<object>> RequestOvertime(OvertimeRequestDto overtimeRequest);
        public Task<ApiResponse<object>> Review(int id, OvertimeReview request);
        public Task<ApiResponse<object>> UpdateOvertimeRequest(int id, UpdateOvertimeDto request);
    }
}
