using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Services
{
    public interface IHolidayService
    {
        public Task<ApiResponse<object>> GetHolidays(int page, int pageSize);
        public Task<ApiResponse<object>> AddHoliday(AddHolidayDto request);
        public Task<ApiResponse<object>> EditHoliday(int id, [FromBody] EditHolidayDto request);
    }
}
