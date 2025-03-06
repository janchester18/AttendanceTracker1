using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class OvertimeConfigController : ControllerBase
    {
        private readonly IOvertimeConfigService _overtimeconfigservice;
        public OvertimeConfigController(IOvertimeConfigService overtimeConfigService)
        {
            _overtimeconfigservice = overtimeConfigService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOvertimeConfig()
        {
            try
            {
                var response = await _overtimeconfigservice.GetOvertimeConfig();
                return Ok(response);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }

        }

        [HttpPut]
        public async Task<IActionResult> UpdateConfig([FromBody] OvertimeConfigDto updatedConfig)
        {
            try
            {
                var response = await _overtimeconfigservice.UpdateConfig(updatedConfig);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
