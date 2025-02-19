using System;
using System.Security.Claims;
using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class OvertimeConfigController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public OvertimeConfigController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOvertimeConfig()
        {
            try
            {
                var config = await _context.OvertimeConfigs.ToListAsync();

                return Ok(ApiResponse<object>.Success(config));
            }
            catch(Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateConfig([FromBody] OvertimeConfigDto updatedConfig)
        {
            try
            {
                var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();

                if (config == null)
                {
                    return BadRequest("Overtime configuration not found.");
                }

                config.OvertimeDailyMax = updatedConfig.OvertimeDailyMax ?? config.OvertimeDailyMax;
                config.BreaktimeMax = updatedConfig.BreaktimeMax ?? config.BreaktimeMax;
                config.OfficeStartTime = updatedConfig.OfficeStartTime ?? config.OfficeStartTime;
                config.OfficeEndTime = updatedConfig.OfficeEndTime ?? config.OfficeEndTime;
                config.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Get the user ID from the JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                var userId = int.Parse(userIdClaim); // Convert string to integer if necessary

                var user = await _context.Users.FindAsync(userId);
                var username = user?.Name ?? "Unknown";

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Config")
                    .Information("{UserName} has updated the config at {Time}", username, DateTime.Now);

                return Ok(ApiResponse<object>.Success(config));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
