using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;

namespace AttendanceTracker1.Services.OvertimeMplService
{
    public interface IOvertimeMplService
    {
        public Task<ApiResponse<object>> GetOvertimeMplRecords(int page, int pageSize, DateTime startDate, DateTime endDate);
        public Task<ApiResponse<object>> GetOvertimeMplById(int id);
        public Task<ApiResponse<object>> GetOvertimeMplRecordsByUser(int userId, int page, int pageSize);
        public Task<ApiResponse<object>> ConvertOvertimeToMpl(int userId, ConvertOvertimeMplDto request);
        public Task<ApiResponse<object>> UpdateOvertimeMplRecord(int id, OvertimeMplUpdateDto request);
    }
}
