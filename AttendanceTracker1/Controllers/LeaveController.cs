﻿using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services.LeaveService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;
        
        public LeaveController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLeaveRequests(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _leaveService.GetLeaveRequests(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetLeaveRequestById(int id)
        {
            try
            {
                var response = await _leaveService.GetLeaveRequestById(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
            
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetLeaveRequestByUserId(int id)
        {
            try
            {
                var response = await _leaveService.GetLeaveRequestByUserId(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("self")]
        [Authorize]
        public async Task<IActionResult> GetSelfLeaveRequest(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _leaveService.GetSelfLeaveRequest(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestLeave([FromBody] RequestLeaveDto request)
        {
            try
            {
                var response = await _leaveService.RequestLeave(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));

            }

        }

        [HttpPut("review/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Review (int id, [FromBody] LeaveReviewDto request)
        {
            try
            {
                var response = await _leaveService.Review(id, request);
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
                var response = await _leaveService.Approve(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("reject/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] LeaveRejectDto request)
        {
            try
            {
                var response = await _leaveService.Reject(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateLeaveRequest(int id, [FromBody] UpdateLeaveDto request)
        {
            try
            {
                var response = await _leaveService.UpdateLeaveRequest(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("cancel/{id}")]
        [Authorize]
        public async Task<IActionResult> CancelLeaveRequest(int id)
        {
            try
            {
                var response = await _leaveService.CancelLeaveRequest(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
