using System.Security.Claims;
using System.Text.RegularExpressions;
using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OvertimeController : ControllerBase
    {
        private readonly IOvertimeService _overtimeService;
        public OvertimeController(IOvertimeService overtimeService)
        {
            _overtimeService = overtimeService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOvertimeRequests(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _overtimeService.GetOvertimeRequests(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOvertimeRequestById(int id)
        {
            try
            {
               var response = await _overtimeService.GetOvertimeRequestById(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetOvertimeRequestByUserId(int id)
        {
            try
            {
                var response = await _overtimeService.GetOvertimeRequestByUserId(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestOvertime([FromBody] OvertimeRequestDto overtimeRequest)
        {
            try
            {
                var response = await _overtimeService.RequestOvertime(overtimeRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("review/{id}")]
        [Authorize (Roles="Admin")]
        public async Task<IActionResult> Review(int id, [FromBody] OvertimeReview request)
        {
            try
            {
                var response = await _overtimeService.Review(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
