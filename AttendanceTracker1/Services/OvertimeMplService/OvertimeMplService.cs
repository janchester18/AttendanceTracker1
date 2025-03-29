using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services.NotificationService;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Security.Claims;

namespace AttendanceTracker1.Services.OvertimeMplService
{
    public class OvertimeMplService : IOvertimeMplService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;

        public OvertimeMplService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, INotificationService notificationService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
        }
        public async Task<ApiResponse<object>> GetOvertimeMplRecords(int page, int pageSize, DateTime startDate, DateTime endDate)
        {
            // Filter records within the cutoff period.
            var query = _context.OvertimeMpls
                .Where(r => r.CutoffStartDate >= startDate && r.CutoffEndDate <= endDate);

            // Get total count for pagination.
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Retrieve the records for the requested page.
            var records = await query
                .OrderByDescending(r => r.CutoffEndDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Prepare the response with data and pagination metadata.
            var responseData = new
            {
                Data = records,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };

            return ApiResponse<object>.Success(responseData, "Overtime MPL records retrieved successfully.");
        }
        public async Task<ApiResponse<object>> GetOvertimeMplById(int id)
        {
            // Attempt to find the record in the database.
            var record = await _context.OvertimeMpls.FindAsync(id);

            // If the record is not found, return a failure response.
            if (record == null)
                return ApiResponse<object>.Failed("Overtime MPL record not found.");

            // Otherwise, return a success response with the record.
            return ApiResponse<object>.Success(record, "Overtime MPL record retrieved successfully.");
        }

        public async Task<ApiResponse<object>> GetOvertimeMplRecordsByUser(int userId, int page, int pageSize)
        {
            // Filter records by the specified user ID.
            var query = _context.OvertimeMpls.Where(r => r.UserId == userId);

            // Get the total count of records for pagination.
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Retrieve the records for the requested page.
            var records = await query
                .OrderByDescending(r => r.CutoffEndDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Prepare the response data with pagination metadata.
            var responseData = new
            {
                OvertimeMpls = records,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };

            return ApiResponse<object>.Success(responseData, "Overtime MPL records for user retrieved successfully.");
        }

        public async Task<ApiResponse<object>> ConvertOvertimeToMpl(int userId, ConvertOvertimeMplDto request)
        {
            // Determine the cutoff period dynamically.
            var now = DateTime.Now;
            DateTime cutoffStart, cutoffEnd;

            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminUsername = admin?.FindFirst(ClaimTypes.Name)?.Value;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return ApiResponse<object>.Success(null, "User not found.");
            var username = user.Name;

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            if (now.Day <= 15)
            {
                // If today is on or before the 15th,
                // the cutoff period is from the 16th of the previous month to the 15th of the current month.
                if (now.Month == 1)
                {
                    cutoffStart = new DateTime(now.Year - 1, 12, 16);
                    cutoffEnd = new DateTime(now.Year, 1, 15);
                }
                else
                {
                    cutoffStart = new DateTime(now.Year, now.Month - 1, 16);
                    cutoffEnd = new DateTime(now.Year, now.Month, 15);
                }
            }
            else
            {
                // If today is after the 15th,
                // the cutoff period is from the 16th of the current month to the 15th of the next month.
                cutoffStart = new DateTime(now.Year, now.Month, 16);
                if (now.Month == 12)
                {
                    cutoffEnd = new DateTime(now.Year + 1, 1, 15);
                }
                else
                {
                    cutoffEnd = new DateTime(now.Year, now.Month + 1, 15);
                }
            }

            // Retrieve total overtime hours for the user within the cutoff period.
            var totalOvertimeHours = await _context.Attendances
                .Where(a => a.UserId == userId
                            && a.Date >= cutoffStart
                            && a.Date <= cutoffEnd)
                .SumAsync(a => a.OvertimeDuration / 60);

            // Calculate the maximum convertible MPL (8 hours per MPL).
            int maxConvertibleMpl = (int)Math.Floor(totalOvertimeHours / 8);

            // Sum up previous MPL conversion records for the same cutoff period.
            var alreadyConvertedMpl = await _context.OvertimeMpls
                .Where(om => om.UserId == userId &&
                             om.CutoffStartDate == cutoffStart &&
                             om.CutoffEndDate == cutoffEnd)
                .SumAsync(om => om.MPLConverted);

            // Determine the remaining convertible MPL.
            int remainingConvertibleMpl = maxConvertibleMpl - alreadyConvertedMpl;

            // Prepare cutoff period data for response.
            var cutoffData = new
            {
                CutoffStartDate = cutoffStart,
                CutoffEndDate = cutoffEnd,
                TotalOvertimeHours = totalOvertimeHours,
                MaxConvertibleMpl = maxConvertibleMpl,
                AlreadyConvertedMpl = alreadyConvertedMpl,
                RemainingConvertibleMpl = remainingConvertibleMpl
            };

            // Check if the new conversion exceeds the remaining convertible MPL.
            if (request.MPLConverted > remainingConvertibleMpl)
                return ApiResponse<object>.Failed(
                    "Requested MPL conversion exceeds the remaining convertible MPL based on overtime hours.", cutoffData);

            // Create a new conversion record.
            var conversionRecord = new OvertimeMpl
            {
                UserId = userId,
                TotalOvertimeHours = (decimal)totalOvertimeHours,
                MPLConverted = request.MPLConverted,
                ResidualOvertimeHours = (decimal)Math.Floor(totalOvertimeHours - ((alreadyConvertedMpl + request.MPLConverted) * 8)),
                CutoffStartDate = cutoffStart,
                CutoffEndDate = cutoffEnd,
                ConversionDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow
            };

            _context.OvertimeMpls.Add(conversionRecord);
            await _context.SaveChangesAsync();

            user.Mpl += conversionRecord.MPLConverted;

            // Include cutoff period data in the successful response.
            var responseData = new
            {
                ConversionRecord = conversionRecord,
            };

            var notificationMessage = $"{adminUsername} has converted your overtime hours for this cutoff to {request.MPLConverted} MPL/s. Your remaining convertible overtime hours is {Math.Floor(conversionRecord.ResidualOvertimeHours)}.";

            var notification = await _notificationService.CreateNotification(
                userId: conversionRecord.UserId,
                title: "Overtime to MPL Conversion",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                type: "MPL Conversion"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Leave")
                .Information("{AdminUserName} has converted the overtime of {UserName} to {Mpl} MPL/s on {Time}", adminUsername, username, request.MPLConverted, DateTime.Now);

            return ApiResponse<object>.Success(responseData, "Overtime successfully converted to MPL.");
        }


        public async Task<ApiResponse<object>> UpdateOvertimeMplRecord(int id, OvertimeMplUpdateDto request)
        {
            throw new NotImplementedException();
        }
    }
}
