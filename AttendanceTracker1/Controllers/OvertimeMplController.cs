using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services.OvertimeMplService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OvertimeMplController : ControllerBase
    {
        private readonly IOvertimeMplService _overtimeMplService;

        public OvertimeMplController(IOvertimeMplService overtimeMplService)
        {
            _overtimeMplService = overtimeMplService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOvertimeMplRecords(int page, int pageSize, DateTime startDate, DateTime endDate)
        {
            try
            {
                var response = await _overtimeMplService.GetOvertimeMplRecords(page, pageSize, startDate, endDate);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOvertimeMplById(int id)
        {
            try
            {
                var response = await _overtimeMplService.GetOvertimeMplById(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetOvertimeMplRecordsByUser(int id, int page, int pageSize)
        {
            try
            {
                var response = await _overtimeMplService.GetOvertimeMplRecordsByUser(id, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPost("{id}")]
        [Authorize]
        public async Task<IActionResult> ConvertOvertimeToMpl(int id, [FromBody] ConvertOvertimeMplDto request)
        {
            try
            {
                var response = await _overtimeMplService.ConvertOvertimeToMpl(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateOvertimeMplRecord(int id, [FromBody] OvertimeMplUpdateDto request)
        {
            try
            {
                var response = await _overtimeMplService.UpdateOvertimeMplRecord(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
