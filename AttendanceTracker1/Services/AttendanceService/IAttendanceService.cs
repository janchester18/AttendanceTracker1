using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;

namespace AttendanceTracker1.Services.AttendanceService
{
    public interface IAttendanceService
    {
        public Task<ApiResponse<object>> GetAttendanceSummary
        (
            int page,
            int pageSize,
            DateTime? startDate = null,
            DateTime? endDate = null
        );

        public Task<ApiResponse<object>> GetFullAttendanceSummary
        (
            int page,
            int pageSize,
            DateTime? startDate = null,
            DateTime? endDate = null
        );
        public Task<ApiResponse<object>> GetAttendances(int page, int pageSize);
        public Task<ApiResponse<object>> GetAttendanceByUser(int id);
        public Task<ApiResponse<object>> GetAttendanceByAttendanceId(int id);
        public Task<ApiResponse<object>> GetSelfAttendance(int page, int pageSize);
        public Task<ApiResponse<object>> ClockIn(ClockInDto clockInDto); // notify concerned user and all admins when late
        public Task<ApiResponse<object>> ClockOut(ClockOutDto clockOutDto); // notify  concerned user and all admins when early out
        public Task<ApiResponse<object>> StartBreak();
        public Task<ApiResponse<object>> EndBreak(); // notify concerned user and all admins when break exceeded the configured time
        public Task<ApiResponse<object>> EditAttendanceRecord(int id, EditAttendanceRecordDto updatedAttendance); // notify the concerned user and all admins
        public Task<ApiResponse<object>> UpdateAttendanceVisibility(int id, UpdateAttendanceVisibilityDto request); 
    }
}
