using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AttendanceTracker1.Services
{
    public class OvertimeConfigService : IOvertimeConfigService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;
        public OvertimeConfigService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, INotificationService notificationService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<object>> GetOvertimeConfig()
        {
            var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();
            if (config == null) return (ApiResponse<object>.Success(null, "Overtime configuration not found."));
            return (ApiResponse<object>.Success(config, "Overtime configuration requested successfully."));
        }
        public async Task<ApiResponse<object>> UpdateConfig(OvertimeConfigDto updatedConfig)
        {
            var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();

            if (config == null) return (ApiResponse<object>.Success(null, "Overtime configuration not found."));

            config.OvertimeDailyMax = updatedConfig.OvertimeDailyMax ?? config.OvertimeDailyMax;
            config.BreakMax = updatedConfig.BreakMax ?? config.BreakMax;
            config.OfficeStartTime = updatedConfig.OfficeStartTime ?? config.OfficeStartTime;
            config.OfficeEndTime = updatedConfig.OfficeEndTime ?? config.OfficeEndTime;
            config.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminUsername = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminUsername) || string.IsNullOrEmpty(adminIdClaim)) return (ApiResponse<object>.Success(null, "Invalid token."));

            var userId = int.Parse(adminIdClaim);

            var notificationMessage = $"{adminUsername} has updated the configuration.";

            var notification = await _notificationService.CreateAdminNotification(
                title: "Configuration Update",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                createdById: userId,
                type: "Configuration Update"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Config")
                .Information("{UserName} has updated the config at {Time}", adminUsername, DateTime.Now);

            return (ApiResponse<object>.Success(config, "Config has been updated successfully."));
        }
    }
}
