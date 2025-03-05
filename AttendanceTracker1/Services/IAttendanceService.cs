using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace AttendanceTracker1.Services
{
    public interface IAttendanceService
    {
        public Task<ApiResponse<object>> GetAttendances(int page, int pageSize);
        public Task<ApiResponse<object>> GetAttendanceByUser(int id);
        public Task<ApiResponse<object>> GetAttendanceByAttendanceId(int id);
        public Task<ApiResponse<object>> ClockIn(ClockInDto clockInDto); // notify concerned user and all admins when late
        public Task<ApiResponse<object>> ClockOut(ClockOutDto clockOutDto); // notify  concerned user and all admins when early out
        public Task<ApiResponse<object>> StartBreak();
        public Task<ApiResponse<object>> EndBreak(); // notify concerned user and all admins when break exceeded the configured time
        public Task<ApiResponse<object>> EditAttendanceRecord(int id, EditAttendanceRecordDto updatedAttendance); // notify the concerned user and all admins
        public Task<ApiResponse<object>> UpdateAttendanceVisibility(int id, UpdateAttendanceVisibilityDto request); 
    }
}
