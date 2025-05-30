﻿using AttendanceTracker1.Models;

namespace AttendanceTracker1.Services.NotificationService
{
    public interface INotificationService
    {
        public Task<ApiResponse<object>> CreateNotification(int userId, string title, string message, string type, string? link = null);
        public Task<ApiResponse<object>> CreateAdminNotification(string title, string message, string type, string? link = null, int? createdById = null);
        public Task<ApiResponse<object>> GetAllNotifications(int page, int pageSize);
        public Task<ApiResponse<object>> GetUserNotifications(int userId, int page, int pageSize);
        public Task<ApiResponse<object>> GetSelfNotificationsAttendance(int page, int pageSize);
        public Task<ApiResponse<object>> GetSelfNotificationsCashAdvance(int page, int pageSize);
        public Task<ApiResponse<object>> View(int notificationId);
        public Task<ApiResponse<object>> DeleteNotification(int notificationId);
        public Task UpdateNotificationLink(int notificationId, string link);
    }
}
