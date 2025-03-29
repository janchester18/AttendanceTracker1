using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AttendanceTracker1.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ApiResponse<object>> CreateNotification(int userId, string title, string message, string type, string? link = null)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return ApiResponse<object>.Success(null, "User does not exist.");

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Link = "",
                CreatedAt = DateTime.Now,
                IsRead = false,
                Status = Notification.VisibilityStatus.Enabled
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            notification.Link = link.Replace("{id}", notification.Id.ToString());

            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();

            return ApiResponse<object>.Success(notification, "Notification created successfully");
        }
        public async Task<ApiResponse<object>> CreateAdminNotification(string title, string message, string type, string? link = null, int? createdById = null)
        {
            // 🔹 Find all admins
            var adminUsers = await _context.Users
                .Where(u => u.Role == "Admin" && u.Id != createdById) // Adjust based on your role logic
                .Select(u => u.Id)
                .ToListAsync();

            if (!adminUsers.Any()) return ApiResponse<object>.Success(null, "User does not exist or is not an admin.");

            var notifications = adminUsers.Select(adminId => new Notification
            {
                UserId = adminId,
                Title = title,
                Message = message,
                Type = type,
                Link = "",
                CreatedAt = DateTime.Now,
                IsRead = false,
                Status = Notification.VisibilityStatus.Enabled
            }).ToList();

            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                notification.Link = link.Replace("{id}", notification.Id.ToString());
            }

            _context.Notifications.UpdateRange(notifications);
            await _context.SaveChangesAsync();

            return ApiResponse<object>.Success(null, "Notification sent to all admins.");
        }

        public async Task<ApiResponse<object>> GetUserNotifications(int userId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var totalRecords = await _context.Notifications
                .Where(n => n.UserId == userId)
                .CountAsync();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderBy(n => n.CreatedAt) // Stable ordering
                .Skip(skip) // Skip the records for the previous pages
                .Take(pageSize) // Limit the number of records to the page size
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    UserName = n.User != null ? n.User.Name : null, // Avoids cyclic reference
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Link = n.Link,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    Status = n.Status.ToString()
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return ApiResponse<object>.Success(new
            {
                notifications,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "User notifications retrieved successfully.");
        }

        public async Task<ApiResponse<object>> GetSelfNotificationsAttendance(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(userIdClaim);

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return ApiResponse<object>.Success(null, $"User with ID {userId} not found.");

            var totalRecords = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Where(n => n.Type != "Cash Advance Request")
                .CountAsync();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Where(n => n.Type != "Cash Advance Request")
                .OrderByDescending(n => n.CreatedAt) // Stable ordering
                .Skip(skip) // Skip the records for the previous pages
                .Take(pageSize) // Limit the number of records to the page size
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    UserName = n.User != null ? n.User.Name : null, // Avoids cyclic reference
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Link = n.Link,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    Status = n.Status.ToString()
                })
                .ToListAsync();

            var unreadCount = await _context.Notifications.Where(n => n.IsRead == false && n.UserId == userId && n.Type != "Cash Advance Request").CountAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return ApiResponse<object>.Success(new
            {
                notifications,
                unreadCount,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "User notifications retrieved successfully.");
        }

        public async Task<ApiResponse<object>> GetSelfNotificationsCashAdvance(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(userIdClaim);

            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                return ApiResponse<object>.Success(null, $"User with ID {userId} not found.");

            var totalRecords = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Where(n => n.Type == "Cash Advance Request")
                .CountAsync();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Where(n => n.Type == "Cash Advance Request")
                .OrderByDescending(n => n.CreatedAt) // Stable ordering
                .Skip(skip) // Skip the records for the previous pages
                .Take(pageSize) // Limit the number of records to the page size
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    UserName = n.User != null ? n.User.Name : null, // Avoids cyclic reference
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Link = n.Link,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    Status = n.Status.ToString()
                })
                .ToListAsync();

            var unreadCount = await _context.Notifications.Where(n => n.IsRead == false && n.UserId == userId && n.Type == "Cash Advance Request").CountAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return ApiResponse<object>.Success(new
            {
                notifications,
                unreadCount,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "User notifications retrieved successfully.");
        }

        public async Task<ApiResponse<object>> GetAllNotifications(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var totalRecords = await _context.Notifications.CountAsync();

            var notifications = await _context.Notifications
                .OrderBy(n => n.CreatedAt) // Stable ordering
                .Skip(skip) // Skip the records for the previous pages
                .Take(pageSize) // Limit the number of records to the page size
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    UserName = n.User != null ? n.User.Name : null, // Avoids cyclic reference
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Link = n.Link,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    Status = n.Status.ToString()
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return ApiResponse<object>.Success(new
            {
                notifications,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "User notifications retrieved successfully.");
        }
        public async Task<ApiResponse<object>> View(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);

            notification.MarkAsRead();
            await _context.SaveChangesAsync();

            var notificationResponse = await _context.Notifications
                .Where(n => n.Id == notificationId)
                .Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    UserName = n.User != null ? n.User.Name : null, // Avoids cyclic reference
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    Link = n.Link,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead,
                    Status = n.Status.ToString()
                }).FirstOrDefaultAsync();
            if (notificationResponse == null) return ApiResponse<object>.Success(null, "Notification not found.");

            return ApiResponse<object>.Success(notificationResponse, "Notification marked as read.");
        }
        public async Task<ApiResponse<object>> DeleteNotification(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return ApiResponse<object>.Success(null, "Notification not found.");

            notification.Status = Notification.VisibilityStatus.Disabled;
            await _context.SaveChangesAsync();

            return ApiResponse<object>.Success(null, "Notification deleted successfully.");
        }

        public async Task UpdateNotificationLink(int notificationId, string link)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.Link = link; // Update the link
                await _context.SaveChangesAsync();
            }
        }
    }
}
