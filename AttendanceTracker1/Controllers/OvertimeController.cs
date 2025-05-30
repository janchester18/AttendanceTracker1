﻿using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services.OvertimeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("self")]
        [Authorize]
        public async Task<IActionResult> GetSelfOvertimeRequest(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _overtimeService.GetSelfOvertimeRequest(page, pageSize);
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

        [HttpPut("reject/{id}")]
        [Authorize (Roles="Admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] OvertimeReview request)
        {
            try
            {
                var response = await _overtimeService.Reject(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var response = await _overtimeService.Approve(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateOvertimeRequest (int id, [FromBody] UpdateOvertimeDto request)
        {
            try
            {
                var response = await _overtimeService.UpdateOvertimeRequest(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelOvertimeRequest(int id)
        {
            try
            {
                var response = await _overtimeService.CancelOvertimeRequest(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
