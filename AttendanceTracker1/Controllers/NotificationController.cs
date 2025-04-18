﻿using AttendanceTracker1.Models;
using AttendanceTracker1.Services.NotificationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateNotification (int userId, string title, string message, string type, string? link = null)
        {
            try
            {
                var response = await _notificationService.CreateNotification(userId, title, message, type, link = null);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("self/attendance")]
        [Authorize]
        public async Task<IActionResult> GetSelfNotificationsAttendance (int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _notificationService.GetSelfNotificationsAttendance(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("self/cash-advance")]
        [Authorize]
        public async Task<IActionResult> GetSelfNotificationsCashAdvance(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _notificationService.GetSelfNotificationsCashAdvance(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserNotifications(int id, int page = 1, int pageSize = 10)
        { 
            try
            {
                var response = await _notificationService.GetUserNotifications(id, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllNotifications(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _notificationService.GetAllNotifications(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("view/{id}")]
        [Authorize]
        public async Task<IActionResult> View (int id)
        {
            try
            {
                var response = await _notificationService.View(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
