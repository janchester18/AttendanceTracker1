using System.Security.Claims;
using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services.AttendanceService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceSummary(int page = 1, int pageSize = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var response = await _attendanceService.GetAttendanceSummary(page, pageSize, startDate, endDate);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("full-summary")]
        [Authorize]
        public async Task<IActionResult> GetFullAttendanceSummary(int page = 1, int pageSize = 10, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var response = await _attendanceService.GetFullAttendanceSummary(page, pageSize, startDate, endDate);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> GetAttendances(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _attendanceService.GetAttendances(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }


        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceByUser(int id)
        {
            try
            {
                var response = await _attendanceService.GetAttendanceByUser(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAttendanceByAttendanceId(int id)
        {
            try
            {
                var response = await _attendanceService.GetAttendanceByAttendanceId(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
            
        }

        [HttpGet("self")]
        [Authorize]
        public async Task<IActionResult> GetSelfAttendance(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _attendanceService.GetSelfAttendance(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPost("clockin")]
        [Authorize]
        public async Task<IActionResult> ClockIn([FromBody] ClockInDto clockInDto)
        {
            try
            {
                var response = await _attendanceService.ClockIn(clockInDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("clockout")]
        [Authorize]
        public async Task<IActionResult> ClockOut([FromBody] ClockOutDto clockOutDto)
        {
            try
            {   
                var response = await _attendanceService.ClockOut(clockOutDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("start-break")]
        [Authorize]
        public async Task<IActionResult> StartBreak()
        {
            try
            {
                var response = await _attendanceService.StartBreak();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }

        }

        [HttpPut("end-break")]
        [Authorize]
        public async Task<IActionResult> EndBreak()
        {
            try
            {
                var response = await _attendanceService.EndBreak();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }

        }


        [HttpPut("edit-attendance/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditAttendanceRecord(int id, [FromBody] EditAttendanceRecordDto updatedAttendance)
        {
            try
            {
                var response = await _attendanceService.EditAttendanceRecord(id, updatedAttendance);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("update-status/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAttendanceVisibility(int id, [FromBody] UpdateAttendanceVisibilityDto request)
        {
            try
            {
                var response = await _attendanceService.UpdateAttendanceVisibility(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
