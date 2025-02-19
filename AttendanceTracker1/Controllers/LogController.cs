using AttendanceTracker1.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LogController> _logger;

        public LogController(ApplicationDbContext context, ILogger<LogController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/logs?page=1&pageSize=50
        [HttpGet]
        public async Task<IActionResult> GetLogs(int page = 1, int pageSize = 50)
        {

                var logs = await _context.Logs
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(logs);
        }
    }
}
