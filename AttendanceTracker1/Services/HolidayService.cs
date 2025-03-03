using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AttendanceTracker1.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HolidayService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ApiResponse<object>> GetHolidays(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var totalRecords = await _context.Holidays.CountAsync();
            var holidays = await _context.Holidays
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return ApiResponse<object>.Success(new
            {
                holidays,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "Holiday data requested successfully.");
        }

        public async Task<ApiResponse<object>> AddHoliday (AddHolidayDto request)
        {
            var holiday = new Holiday
            {
                Name = request.Name,
                Date = request.Date,
                IsPaid = request.IsPaid,
                IsNational = request.IsNational,
                Type = request.Type,
                UpdatedAt = DateTime.Now
            };

            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();

            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminUsername = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminIdClaim))
            {
                return (ApiResponse<object>.Success(null, "Invalid token."));
            }

            var userId = int.Parse(adminIdClaim);

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
               .ForContext("Type", "Holiday")
               .Information("{UserName} has added holiday {Id} at {Time}", adminUsername, holiday.Id, DateTime.Now);

            return (ApiResponse<object>.Success(holiday, $"Holiday successfully created."));
        }

        public async Task<ApiResponse<object>> EditHoliday(int id, [FromBody] EditHolidayDto request)
        {
            var holiday = await _context.Holidays.FirstOrDefaultAsync(h => h.Id == id);

            if (holiday == null)
            {
                return (ApiResponse<object>.Success(null, "Holiday not found."));
            }

            holiday.Name = request.Name ?? holiday.Name;
            holiday.Date = request.Date ?? holiday.Date;
            holiday.IsPaid = request.IsPaid ?? holiday.IsPaid;
            holiday.IsNational = request.IsNational ?? holiday.IsNational;
            holiday.Type = request.Type ?? holiday.Type;
            holiday.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminUsername = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminIdClaim))
            {
                return (ApiResponse<object>.Success(null, "Invalid token."));
            }

            var userId = int.Parse(adminIdClaim);

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
               .ForContext("Type", "Holiday")
               .Information("{UserName} has edited holiday {Id} at {Time}", adminUsername, holiday.Id, DateTime.Now);

            var response = new
            {
                name = holiday.Name,
                date = holiday.Date,
                isPaid = holiday.IsPaid,
                isNational = holiday.IsNational,
                type = holiday.Type,
                updatedAt = holiday.UpdatedAt
            };

            return (ApiResponse<object>.Success(response, "Holiday has been updated successfully."));
        }
    }
}
