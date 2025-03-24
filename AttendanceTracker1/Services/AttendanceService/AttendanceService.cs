﻿using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services.NotificationService;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AttendanceTracker1.Services.AttendanceService
{
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;

        public AttendanceService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, INotificationService notificationService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<object>> GetAttendances(int page, int pageSize)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var role = user?.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role)) return ApiResponse<object>.Success(null, "Invalid token");

            var skip = (page - 1) * pageSize;


            var totalRecords = await _context.Attendances.CountAsync();

            var attendances = await _context.Attendances
                .Include(a => a.User)
                .Where(a => role == "Admin" || a.VisibilityStatus == VisibilityStatus.Enabled)
                .OrderByDescending(a => a.VisibilityStatus == VisibilityStatus.Enabled) 
                .ThenByDescending(a => a.Date)
                .Skip(skip)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    User = new
                    {
                        a.User.Name,
                        a.User.Email
                    },
                    a.Date,
                    a.ClockIn,
                    a.ClockOut,
                    a.BreakStart,
                    a.BreakFinish,
                    a.FormattedWorkDuration,
                    a.FormattedBreakDuration,
                    a.FormattedOvertimeDuration,
                    a.FormattedLateDuration,
                    a.FormattedNightDifDuration,
                    Status = a.Status.ToString(),
                    a.Remarks,
                    VisibilityStatus = a.VisibilityStatus.ToString()
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var response = ApiResponse<object>.Success(new
            {
                attendances,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "Attendance data request successful.");

            return response;
        }
        public async Task<ApiResponse<object>> GetAttendanceByUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return ApiResponse<object>.Success(null, "User not found.");

            var userContext = _httpContextAccessor.HttpContext?.User;
            var role = userContext?.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role)) return ApiResponse<object>.Success(null, "Invalid token");

            IQueryable<Attendance> query = _context.Attendances
               .Include(a => a.User);

            if (role == "Employee")
            {
                query = query.Where(a => a.VisibilityStatus == VisibilityStatus.Enabled);
            }

            // Order by VisibilityStatus (Disabled at the end) then by CreatedAt
            query = query.OrderBy(a => a.VisibilityStatus == VisibilityStatus.Disabled)
                         .ThenBy(a => a.CreatedAt);

            var totalRecords = await query.Where(a => a.UserId == id).CountAsync();

            var attendance = await query
            .Where(a => a.UserId == id)
            .Include(a => a.User)
            .Select(a => new
            {

                a.UserId,
                a.Id,
                User = new
                {
                    a.User.Name,
                    a.User.Email
                },
                a.Date,
                a.ClockIn,
                a.ClockOut,
                a.BreakStart,
                a.BreakFinish,
                a.FormattedWorkDuration,
                a.FormattedOvertimeDuration,
                a.FormattedLateDuration,
                Status = a.Status.ToString(),
                a.Remarks,
                VisibilityStatus = role == "Admin" ? a.VisibilityStatus.ToString() : null
            })
                .ToListAsync();

            if (totalRecords == 0) return ApiResponse<object>.Success(null, "No attendance records for this user.");

            return ApiResponse<object>.Success(attendance, "Attendance data request successful.");
        }

        public async Task<ApiResponse<object>> GetSelfAttendance(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;

            var userContext = _httpContextAccessor.HttpContext?.User;
            var userId = int.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int id) ? id : 0;
            var role = userContext?.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role) || userId == 0) return ApiResponse<object>.Success(null, "Invalid token");

            IQueryable<Attendance> query = _context.Attendances
               .Include(a => a.User);

            if (role == "Employee")
            {
                query = query.Where(a => a.VisibilityStatus == VisibilityStatus.Enabled);
            }

            // Order by VisibilityStatus (Disabled at the end) then by CreatedAt
            query = query.OrderBy(a => a.VisibilityStatus == VisibilityStatus.Disabled)
                         .ThenBy(a => a.CreatedAt);

            var totalRecords = await query.Where(a => a.UserId == id).CountAsync();

            var attendance = await query
            .Where(a => a.UserId == userId)
            .Skip(skip)
            .Take(pageSize)
            .OrderByDescending(a => a.Date)
            .Include(a => a.User)
            .Select(a => new
            {

                a.UserId,
                a.Id,
                User = new
                {
                    a.User.Name,
                    a.User.Email
                },
                a.Date,
                a.ClockIn,
                a.ClockOut,
                a.BreakStart,
                a.BreakFinish,
                a.FormattedWorkDuration,
                a.FormattedBreakDuration,
                a.FormattedOvertimeDuration,
                a.FormattedNightDifDuration,
                a.FormattedLateDuration,
                Status = a.Status.ToString(),
                a.Remarks,
                VisibilityStatus = role == "Admin" ? a.VisibilityStatus.ToString() : null
            })
                .ToListAsync();

            if (totalRecords == 0) return ApiResponse<object>.Success(null, "No attendance records for this user.");

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var response = ApiResponse<object>.Success(new
            {
                attendance,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "Attendance data request successful.");

            return response;
        }

        public async Task<ApiResponse<object>> GetAttendanceByAttendanceId(int id)
        {
            var userContext = _httpContextAccessor.HttpContext?.User;
            var role = userContext?.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role)) return ApiResponse<object>.Success(null, "Invalid token");

            IQueryable<Attendance> query = _context.Attendances
               .Include(a => a.User);

            if (role == "Employee") query = query.Where(a => a.VisibilityStatus == VisibilityStatus.Enabled);

            // Order by VisibilityStatus (Disabled at the end) then by CreatedAt
            query = query.OrderBy(a => a.VisibilityStatus == VisibilityStatus.Disabled)
                         .ThenBy(a => a.CreatedAt);

            var attendance = await query
            .Where(a => a.Id == id)
            .Include(a => a.User)
            .Select(a => new
            {
                a.Id, // Attendance ID
                a.UserId,
                User = new
                {
                    a.User.Name,
                    a.User.Email
                },
                a.Date,
                a.ClockIn,
                a.ClockOut,
                a.BreakStart,
                a.BreakFinish,
                a.FormattedWorkDuration,
                a.FormattedOvertimeDuration,
                a.FormattedLateDuration,
                Status = a.Status.ToString(),
                a.Remarks,
                VisibilityStatus = role == "Admin" ? a.VisibilityStatus.ToString() : null
            })
            .FirstOrDefaultAsync();

            if (attendance == null)
            {
                return ApiResponse<object>.Success("Attendance record not found.");
            }

            return ApiResponse<object>.Success(attendance, "Attendance data request successful.");
        }
        public async Task<ApiResponse<object>> ClockIn(ClockInDto clockInDto)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim)) return ApiResponse<object>.Success(null, "Invalid token");

            var userId = int.Parse(userIdClaim);

            var today = DateTime.Now.Date;

            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today);

            if (existingAttendance != null && existingAttendance.ClockIn != default) 
                return ApiResponse<object>.Success(null, "You have already clocked in today.");

            DateTime? parsedClockOut = null;
            if (!string.IsNullOrEmpty(clockInDto.ClockOut) &&
                DateTime.TryParse(clockInDto.ClockOut, out DateTime tempClockOut))
            {
                parsedClockOut = tempClockOut;
            }

            var attendance = new Attendance
            {
                UserId = userId,
                Date = today,
                ClockIn = DateTime.Now,
                ClockOut = parsedClockOut,
                Status = AttendanceStatus.Present,
                Remarks = clockInDto.Remarks,
                ClockInLatitude = clockInDto.ClockInLatitude,
                ClockInLongitude = clockInDto.ClockInLongitude,
            };

            //for testing dummy clock in time
            //if (DateTime.TryParse(clockInDto.ClockIn, out DateTime parsedClockIn))
            //{
            //    attendance.ClockIn = parsedClockIn;
            //}
            //else
            //{
            //    return Ok(ApiResponse<object>.Success(null, "Invalid clock-in time format."));
            //}

            var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();
            var clockInTime = attendance.ClockIn.TimeOfDay;

            string message = "Clock-in recorded successfully.";

            var officeStartDateTime = today.Add(config.OfficeStartTime);
            var lateDuration = (attendance.ClockIn - officeStartDateTime).TotalMinutes; // Store in minutes

            if (lateDuration > 0)
            {
                attendance.Status = AttendanceStatus.Late;
                attendance.LateDuration = lateDuration; // Store in minutes

                int lateHours = (int)(lateDuration / 60);
                int lateMinutes = (int)(lateDuration % 60);

                message = $"Clock-in recorded successfully. However, you are late by {lateHours}h {lateMinutes}m.";

                var adminNotificationMessage = $"{username} has clocked in {lateHours}h {lateMinutes}m late.";
                var employeeNotificationMessage = $"You clocked in {lateHours}h {lateMinutes}m late.";

                var adminNotification = await _notificationService.CreateAdminNotification(
                    title: "Employee Late Clock-in",
                    message: adminNotificationMessage,
                    link: "/api/notification/view/{id}",
                    type: "Attendance Alert"
                );

                var employeeNotification = await _notificationService.CreateNotification(
                    userId: attendance.UserId,
                    title: "Late Clock-in",
                    message: employeeNotificationMessage,
                    link: "/api/notification/view/{id}",
                    type: "Attendance Alert"
                );
            }

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            var response = ApiResponse<object>.Success(new
            {
                attendanceId = attendance.Id,
                clockinLatitude = attendance.ClockInLatitude,
                clockinLongitude = attendance.ClockInLongitude
            }, message);

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Attendance")
                .Information("{UserName} clocked in at {Time}", username, DateTime.Now);

            return response;
        }
        public async Task<ApiResponse<object>> ClockOut(ClockOutDto clockOutDto)
        {
            var userContext = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = userContext?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = userContext?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token");
            

            var userId = int.Parse(userIdClaim);

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == DateTime.Today);

            if (attendance == null) 
                return ApiResponse<object>.Success(null, "Attendance not found");
            if (attendance.ClockOut.HasValue) 
                return ApiResponse<object>.Success(null, "You have already clocked out.");

            //for testing dummy clock out time
            //if (DateTime.TryParse(clockOutDto.ClockOut, out DateTime parsedClockOut))
            //{
            //    attendance.ClockOut = parsedClockOut;
            //}
            //else
            //{
            //    return Ok(ApiResponse<object>.Success(null, "Invalid clock-out time format."));
            //}

            attendance.ClockOut = DateTime.Now;
            attendance.ClockOutLatitude = clockOutDto.ClockOutLatitude;
            attendance.ClockOutLongitude = clockOutDto.ClockOutLongitude;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) 
                return ApiResponse<object>.Success(null, "User not found");

            var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();
            if (config == null) 
                return ApiResponse<object>.Success(null, "Overtime configuration not found.");

            // Break time calculation in minutes
            int breakMinutes = attendance.BreakStart.HasValue && attendance.BreakFinish.HasValue
                ? (int)(attendance.BreakFinish.Value - attendance.BreakStart.Value).TotalMinutes
                : 0;

            // Work period
            DateTime workStart = attendance.ClockIn;
            DateTime workEnd = attendance.ClockOut.Value;

            // Night differential calculation
            DateTime nightStart = DateTime.Today.Add(config.NightDifStartTime);
            DateTime nightEnd = config.NightDifEndTime > config.NightDifStartTime
                ? DateTime.Today.Add(config.NightDifEndTime)
                : DateTime.Today.AddDays(1).Add(config.NightDifEndTime);

            // Calculate effective night differential period
            DateTime effectiveNightStart = workStart > nightStart ? workStart : nightStart;
            DateTime effectiveNightEnd = workEnd < nightEnd ? workEnd : nightEnd;

            int nightDifferentialMinutes = effectiveNightStart < effectiveNightEnd
                ? (int)(effectiveNightEnd - effectiveNightStart).TotalMinutes
                : 0;

            attendance.NightDifDuration = nightDifferentialMinutes;
            user.AccumulatedNightDifferential += nightDifferentialMinutes;

            // Total work duration in minutes
            int totalWorkMinutes = (int)(workEnd - workStart).TotalMinutes - breakMinutes;
            int totalBreakDuration = (int)(attendance.BreakFinish - attendance.BreakStart).Value.TotalMinutes;
            int regularWorkMinutes = (int)((config.OfficeEndTime - config.OfficeStartTime).TotalMinutes - totalBreakDuration);

            int actualOvertimeMinutes = Math.Max(0, totalWorkMinutes - regularWorkMinutes);

            var approvedOvertime = await _context.Overtimes
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Date.Date == attendance.ClockIn.Date && o.Status == OvertimeRequestStatus.Approved);

            int approvedOvertimeMinutes = 0;

            if (approvedOvertime != null)
            {
                approvedOvertimeMinutes = (int)Math.Min(actualOvertimeMinutes,
                    Math.Min((approvedOvertime.EndTime - approvedOvertime.StartTime).TotalMinutes, config.OvertimeDailyMax));
            }

            var message = "You have clocked out successfully.";

            if (attendance.ClockOut.Value.TimeOfDay < config.OfficeEndTime)
            {
                TimeSpan missedWorkDuration = config.OfficeEndTime - attendance.ClockOut.Value.TimeOfDay;

                int missedHours = (int)missedWorkDuration.TotalHours;
                int missedMinutes = missedWorkDuration.Minutes;

                message = $"Clock-out recorded successfully. However, you clocked out {missedHours}h {missedMinutes}m early.";

                var adminNotificationMessage = $"{username} has clocked out {missedHours}h {missedMinutes}m early.";
                var employeeNotificationMessage = $"You clocked out {missedHours}h {missedMinutes}m early.";

                var adminNotification = await _notificationService.CreateAdminNotification(
                    title: "Employee Early Clock-out",
                    message: adminNotificationMessage,
                    link: "/api/notification/view/{id}",
                    type: "Attendance Alert"
                );

                var employeeNotification = await _notificationService.CreateNotification(
                    userId: attendance.UserId,
                    title: "Early Clock-out",
                    message: employeeNotificationMessage,
                    link: "/api/notification/view/{id}",
                    type: "Attendance Alert"
                );
            }

            // Store the approved overtime duration in attendance
            attendance.OvertimeDuration = approvedOvertimeMinutes;

            user.AccumulatedOvertime += approvedOvertimeMinutes;

            await _context.SaveChangesAsync();

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Attendance")
                .Information("{UserName} clocked out at {Time}", username, DateTime.Now);

            // Format Minutes to "Xh Ym"
            string FormatMinutes(double minutes) => $"{minutes / 60}h {minutes % 60}m";

            return ApiResponse<object>.Success(new
            {
                totalWorkTime = FormatMinutes(totalWorkMinutes),
                approvedOvertime = approvedOvertime != null,
                approvedOvertimeDuration = FormatMinutes(approvedOvertimeMinutes),
                actualOvertimeWorked = FormatMinutes(actualOvertimeMinutes),
                overtimeAdded = FormatMinutes(approvedOvertimeMinutes),
                nightDifferential = FormatMinutes(nightDifferentialMinutes),
                newAccumulatedOvertime = FormatMinutes(user.AccumulatedOvertime),
                newAccumulatedNightDifferential = FormatMinutes(user.AccumulatedNightDifferential)
            }, message);
        }
        public async Task<ApiResponse<object>> StartBreak()
        {
            var userContext = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = userContext?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = userContext?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token");

            var userId = int.Parse(userIdClaim);

            var user = await _context.Users.FindAsync(userId);

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == DateTime.Today);

            if (attendance == null)
                return ApiResponse<object>.Success(null, "Attendance not found.");

            if (attendance.ClockOut.HasValue)
                return ApiResponse<object>.Success(null, "Cannot start break because you are already clocked out.");

            if (attendance.BreakStart.HasValue || attendance.BreakFinish.HasValue)
                return ApiResponse<object>.Success(null, "Break has already been started or ended.");

            attendance.BreakStart = DateTime.Now;
            await _context.SaveChangesAsync();

            var message = "Break has started.";

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Attendance")
                .Information("{UserName} started break at {Time}", username, DateTime.Now);

            return ApiResponse<object>.Success(new { attendance.BreakStart }, message);
        }
        public async Task<ApiResponse<object>> EndBreak()
        {
            var userContext = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = userContext?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = userContext?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token");

            var userId = int.Parse(userIdClaim);

            var user = await _context.Users.FindAsync(userId);

            // Get today's attendance record for the user
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == DateTime.Today);

            var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();

            if (attendance == null)
                return ApiResponse<object>.Success(null, "Attendance not found.");

            if (!attendance.BreakStart.HasValue)
                return ApiResponse<object>.Success(null, "Cannot end break because break has not been started.");

            if (attendance.ClockOut.HasValue)
                return ApiResponse<object>.Success(null, "Cannot end break because you are already clocked out.");

            if (attendance.BreakFinish.HasValue)
                return ApiResponse<object>.Success(null, "Break has already been ended.");

            attendance.BreakFinish = DateTime.Now;
            await _context.SaveChangesAsync();

            var message = "Break has ended.";
            double breakDuration = (attendance.BreakFinish.Value.TimeOfDay - attendance.BreakStart.Value.TimeOfDay).TotalMinutes;

            if (breakDuration > config.BreakMax)
            {
                double overBreakDuration = breakDuration - config.BreakMax;

                int Hours = (int)overBreakDuration / 60;
                int Minutes = (int)overBreakDuration % 60;

                message = $"Break has ended. However, you exceeded the maximum break time by {Hours}h {Minutes}m.";

                var adminNotificationMessage = $"{username} exceeded the maximum break time by {Hours}h {Minutes}m.";
                var employeeNotificationMessage = $"You exceeded the maximum break time by {Hours}h {Minutes}m.";

                var adminNotification = await _notificationService.CreateAdminNotification(
                    title: "Employee Break Time Exceeded",
                    message: adminNotificationMessage,
                    link: "/api/notification/view/{id}",
                    type: "Attendance Alert"
                );

                var employeeNotification = await _notificationService.CreateNotification(
                    userId: attendance.UserId,
                    title: "Break Time Exceeded",
                    message: employeeNotificationMessage,
                    link: "/api/notification/view/{id}",
                    type: "Attendance Alert"
                );
            }

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Attendance")
                .Information("{UserName} ended break at {Time}", username, DateTime.Now);

            return ApiResponse<object>.Success(new
            {
                breakDuration = attendance.FormattedBreakDuration // Ensure this property exists in your model
            }, "Break has ended.");
        }
        public async Task<ApiResponse<object>> EditAttendanceRecord(int id, EditAttendanceRecordDto updatedAttendance)
        {
            var userContext = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = userContext?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminName = userContext?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminName) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var adminId = int.Parse(userIdClaim);
            var admin = await _context.Users.FindAsync(adminId);

            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
                return ApiResponse<object>.Success(null, "Attendance Record not Found");

            var user = await _context.Users.FindAsync(attendance.UserId);
            if (user == null)
                return ApiResponse<object>.Success(null, "User not found.");

            // ✅ Update ClockIn, ClockOut, BreakStart, BreakFinish
            if (!string.IsNullOrWhiteSpace(updatedAttendance.ClockIn) && DateTime.TryParse(updatedAttendance.ClockIn, out DateTime clockInValue))
                attendance.ClockIn = clockInValue;

            if (!string.IsNullOrWhiteSpace(updatedAttendance.ClockOut) && DateTime.TryParse(updatedAttendance.ClockOut, out DateTime clockOutValue))
                attendance.ClockOut = clockOutValue;

            if (!string.IsNullOrWhiteSpace(updatedAttendance.BreakStart) && DateTime.TryParse(updatedAttendance.BreakStart, out DateTime breakStartValue))
                attendance.BreakStart = breakStartValue;

            if (!string.IsNullOrWhiteSpace(updatedAttendance.BreakFinish) && DateTime.TryParse(updatedAttendance.BreakFinish, out DateTime breakFinishValue))
                attendance.BreakFinish = breakFinishValue;

            // ✅ Get Overtime & Night Differential Config
            var config = await _context.OvertimeConfigs.FirstOrDefaultAsync();
            if (config == null)
                return ApiResponse<object>.Success(null, "Overtime configuration not found.");

            DateTime workStart = attendance.ClockIn;
            DateTime workEnd = attendance.ClockOut ?? workStart;
            TimeSpan totalWorkDuration = workEnd - workStart;

            // ✅ Recalculate Late Status and Late Duration
            bool isLate = workStart.TimeOfDay > config.OfficeStartTime;
            attendance.Status = isLate ? AttendanceStatus.Late : AttendanceStatus.Present;

            int lateDurationMinutes = 0;
            if (isLate)
            {
                lateDurationMinutes = (int)(workStart.TimeOfDay - config.OfficeStartTime).TotalMinutes;
            }
            // Assuming Attendance has a property LateDuration (in minutes)
            attendance.LateDuration = lateDurationMinutes;

            // ✅ Calculate Break Duration
            TimeSpan breakDuration = TimeSpan.Zero;
            if (attendance.BreakStart.HasValue && attendance.BreakFinish.HasValue)
                breakDuration = attendance.BreakFinish.Value - attendance.BreakStart.Value;

            // ✅ Fetch Approved Overtime Request
            var approvedOvertime = await _context.Overtimes
                .FirstOrDefaultAsync(o => o.UserId == attendance.UserId
                                           && o.Date.Date == attendance.ClockIn.Date
                                           && o.Status == OvertimeRequestStatus.Approved);

            int regularWorkMinutes = (int)(config.OfficeEndTime - config.OfficeStartTime).TotalMinutes;
            int totalWorkMinutes = (int)(totalWorkDuration.TotalMinutes - breakDuration.TotalMinutes);
            int actualOvertimeMinutes = Math.Max(0, totalWorkMinutes - regularWorkMinutes);

            int approvedOvertimeMinutes = 0;
            TimeSpan? overtimeStart = null;
            TimeSpan? overtimeEnd = null;

            // Only calculate approved overtime if an approved request exists.
            if (approvedOvertime != null)
            {
                approvedOvertimeMinutes = Math.Min(actualOvertimeMinutes,
                    (int)Math.Min((approvedOvertime.EndTime - approvedOvertime.StartTime).TotalMinutes, config.OvertimeDailyMax));
                overtimeStart = approvedOvertime.StartTime;
                overtimeEnd = approvedOvertime.EndTime;
            }

            // ✅ Debugging Approved Overtime
            Serilog.Log.Information("Approved Overtime: {OvertimeStart} - {OvertimeEnd}", overtimeStart, overtimeEnd);

            // ✅ Night Differential Calculation (only if approved overtime exists)
            int nightDiffMinutes = 0;
            if (approvedOvertime != null && overtimeStart.HasValue && overtimeEnd.HasValue)
            {
                TimeSpan nightDiffStart = config.NightDifStartTime; // e.g., 22:00:00
                TimeSpan nightDiffEnd = config.NightDifEndTime;     // e.g., 06:00:00

                TimeSpan otStart = overtimeStart.Value;
                TimeSpan otEnd = overtimeEnd.Value;

                // ✅ Log values
                Serilog.Log.Information("OT Start: {OtStart}, OT End: {OtEnd}", otStart, otEnd);
                Serilog.Log.Information("Night Diff Range: {NightStart} - {NightEnd}", nightDiffStart, nightDiffEnd);

                // Check if there is any overlap between approved overtime and the night diff period.
                if (otEnd > nightDiffStart || otStart < nightDiffEnd)
                {
                    TimeSpan nightShiftStart = (otStart >= nightDiffStart) ? otStart : nightDiffStart;
                    TimeSpan nightShiftEnd = (otEnd <= nightDiffEnd) ? otEnd : nightDiffEnd;

                    // ✅ Log computed values
                    Serilog.Log.Information("Computed Night Shift Start: {Start}, Night Shift End: {End}", nightShiftStart, nightShiftEnd);

                    if (nightShiftStart < nightShiftEnd)
                    {
                        nightDiffMinutes = (int)(nightShiftEnd - nightShiftStart).TotalMinutes;
                        Serilog.Log.Information("Night Differential Calculated: {Minutes} minutes", nightDiffMinutes);
                    }
                    else
                    {
                        Serilog.Log.Warning("Night Shift Start is not less than Night Shift End. No ND calculated.");
                    }
                }
            }
            // If no approved overtime, both overtime and night differential remain 0.

            // ✅ Store Overtime, Night Differential, and Late Duration in Attendance
            attendance.OvertimeDuration = approvedOvertimeMinutes;
            attendance.NightDifDuration = nightDiffMinutes;

            // ✅ Save Changes
            await _context.SaveChangesAsync();

            // ✅ Build Response
            var response = new Dictionary<string, object>
    {
        { "totalBreakHours", breakDuration.ToString(@"hh\:mm") },
        { "totalWorkHours", totalWorkDuration.ToString(@"hh\:mm") },
        { "overtimeMinutes", attendance.OvertimeDuration },
        { "nightDiffMinutes", attendance.NightDifDuration },
        { "lateDurationMinutes", attendance.LateDuration },
        { "status", attendance.Status }
    };

            var attendanceDate = attendance.ClockIn.Date.ToString("MMMM dd, yyyy");

            var adminNotificationMessage = $"{adminName} has edited the attendance record of {user.Name} for {attendanceDate}.";
            var employeeNotificationMessage = $"{adminName} has edited your attendance record for {attendanceDate}.";

            await _notificationService.CreateAdminNotification(
                title: "Attendance Record Update",
                message: adminNotificationMessage,
                link: "/api/notification/view/{id}",
                createdById: adminId,
                type: "Attendance Update"
            );

            await _notificationService.CreateNotification(
                userId: attendance.UserId,
                title: "Attendance Record Update",
                message: employeeNotificationMessage,
                link: "/api/notification/view/{id}",
                type: "Attendance Update"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Attendance")
                .Information("Attendance {Id} has been edited by {AdminName}", id, adminName);

            return ApiResponse<object>.Success(response, "Attendance Record updated successfully.");
        }


        public async Task<ApiResponse<object>> UpdateAttendanceVisibility(int id, UpdateAttendanceVisibilityDto request)
        {
            var adminContext = _httpContextAccessor.HttpContext?.User;
            var adminName = adminContext.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(adminName)) 
                return ApiResponse<object>.Success(null, "Invalid token.");

            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) 
                return ApiResponse<object>.Success(null, "Attendance Record not Found");
            if (attendance.VisibilityStatus == request.VisibilityStatus) 
                return ApiResponse<object>.Success(null, "Visibility status is already set to the requested value.");
            if (request.VisibilityStatus != VisibilityStatus.Disabled && request.VisibilityStatus != VisibilityStatus.Enabled) 
                return ApiResponse<object>.Success(null, "Invalid visibility status.");

            var action = request.VisibilityStatus.ToString();

            attendance.VisibilityStatus = request.VisibilityStatus;
            await _context.SaveChangesAsync();

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "Attendance")
                .Information("Attendance {Id} has been {Action} by {AdminName}", id, action, adminName);

            return ApiResponse<object>.Success(null, "Attendance record visibility status updated successfully.");
        }
    }
}
