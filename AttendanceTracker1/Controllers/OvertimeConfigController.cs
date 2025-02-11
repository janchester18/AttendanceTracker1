using AttendanceTracker1.Data;
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
            var config = await _context.OvertimeConfigs.ToListAsync();

            return Ok(config);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateConfig([FromBody] OvertimeConfig updatedConfig)
        {
            var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();

            if (config == null)
            {
                return BadRequest("Overtime configuration not found.");
            }

            config.OvertimeDailyMax = updatedConfig.OvertimeDailyMax;
            config.BreaktimeMax = updatedConfig.BreaktimeMax;
            config.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(config);
        }
    }
}
