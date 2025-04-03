using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services.NotificationService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Security.Claims;

namespace AttendanceTracker1.Services.CashAdvanceRequestService
{
    public class CashAdvanceRequestService : ICashAdvanceRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;

        public CashAdvanceRequestService(
            ApplicationDbContext context, 
            IHttpContextAccessor httpContextAccessor, 
            INotificationService notificationService
            )
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<object>> GetCashAdvanceRequests(int page, int pageSize, string keyword = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var skip = (page - 1) * pageSize;
            var query = _context.CashAdvanceRequests
                .Include(x => x.User)
                .Include(x => x.Approver)
                .Include(x => x.PaymentSchedule)
                .AsQueryable(); // Start query

            // ✅ Apply search filter (Keyword)
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.User.Name.Contains(keyword));
            }

            // ✅ Apply date range filter (Start Date & End Date)
            if (startDate.HasValue)
            {
                query = query.Where(x => x.RequestDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(x => x.RequestDate <= endDate.Value);
            }

            var totalRecords = await query.CountAsync(); // ✅ Count after filtering

            var cashAdvanceRequests = await query
                .OrderByDescending(x => x.RequestDate)
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new CashAdvanceResponseDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User.Name,
                    UserEmail = x.User.Email,
                    Amount = x.Amount,
                    NeededDate = x.NeededDate,
                    MonthsToPay = x.MonthsToPay,
                    PaymentSchedule = x.PaymentSchedule.ToList(),
                    RequestStatus = x.RequestStatus,
                    RequestDate = x.RequestDate,
                    Reason = x.Reason,
                    ReviewedBy = x.ReviewedBy,
                    ApproverName = x.Approver.Name,
                    RejectionReason = x.RejectionReason,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            // ✅ Get total count (without pagination)
            var allCount = await _context.CashAdvanceRequests.CountAsync();

            // ✅ Pending count
            var pendingCount = await _context.CashAdvanceRequests
                .Where(p => p.Status == CashAdvanceRequestStatus.Pending)
                .CountAsync();

            DateTime today = DateTime.Now;
            int currentMonth = today.Month;
            int currentYear = today.Year;

            // ✅ Approved this month count
            var approvedThisMonth = await _context.CashAdvanceRequests
                .Where(p => p.Status == CashAdvanceRequestStatus.Approved &&
                            p.UpdatedAt.Month == currentMonth &&
                            p.UpdatedAt.Year == currentYear)
                .CountAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var response = ApiResponse<object>.Success(new
            {
                cashAdvanceRequests,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1,
                approvedThisMonth,
                allCount,
                pendingCount
            }, "Data request successful.");

            return response;
        }


        public async Task<ApiResponse<object>> GetSelfCashAdvanceRequests(int page, int pageSize)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(userIdClaim);

            var skip = (page - 1) * pageSize;
            var totalRecords = await _context.CashAdvanceRequests.CountAsync();

            var cashAdvanceRequests = await _context.CashAdvanceRequests
                .AsSplitQuery() // Use AsSplitQuery to split the query
                .Include(x => x.User)
                .Include(x => x.Approver)
                .Include(x => x.PaymentSchedule)
                .OrderByDescending(x => x.RequestDate) // Add an OrderBy clause here
                .Where(x => x.UserId == userId)
                .Skip(skip)
                .Take(pageSize)
                .Select(x => new CashAdvanceResponseDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User.Name,
                    UserEmail = x.User.Email,
                    Amount = x.Amount,
                    NeededDate = x.NeededDate,
                    MonthsToPay = x.MonthsToPay,
                    PaymentSchedule = x.PaymentSchedule.ToList(),
                    RequestStatus = x.RequestStatus,
                    RequestDate = x.RequestDate,
                    Reason = x.Reason,
                    ReviewedBy = x.ReviewedBy,
                    ApproverName = x.Approver.Name,
                    RejectionReason = x.RejectionReason,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var response = ApiResponse<object>.Success(new
            {
                cashAdvanceRequests,
                totalRecords,
                totalPages,
                currentPage = page,
                pageSize,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }, "Data request successful.");

            return response;
        }

        public async Task<ApiResponse<object>> GetCashAdvanceRequestById(int id)
        {
            var cashAdvanceRequest = await _context.CashAdvanceRequests
                .Include(x => x.User)
                .Include(x => x.Approver)
                .Select(x => new CashAdvanceResponseDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User.Name,
                    UserEmail = x.User.Email,
                    Amount = x.Amount,
                    NeededDate = x.NeededDate,
                    MonthsToPay = x.MonthsToPay,
                    PaymentSchedule = x.PaymentSchedule.ToList(),
                    RequestStatus = x.RequestStatus,
                    RequestDate = x.RequestDate,
                    Reason = x.Reason,
                    ReviewedBy = x.ReviewedBy,
                    ApproverName = x.Approver.Name,
                    RejectionReason = x.RejectionReason,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            return cashAdvanceRequest == null
                ? ApiResponse<object>.Success(null, "Cash advance request not found.")
                : ApiResponse<object>.Success(cashAdvanceRequest, "Data request successful.");
        }
        public async Task<ApiResponse<object>> GetCashAdvanceRequestByUserId(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return ApiResponse<object>.Success(null, "User not found.");

            var cashAdvanceRequests = await _context.CashAdvanceRequests
                .Include(x => x.User)
                .Include(x => x.Approver)
                .Where(x => x.UserId == id)
                .OrderByDescending(x => x.RequestDate)
                .Select(x => new CashAdvanceResponseDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    UserName = x.User.Name,
                    UserEmail = x.User.Email,
                    Amount = x.Amount,
                    NeededDate = x.NeededDate,
                    MonthsToPay = x.MonthsToPay,
                    PaymentSchedule = x.PaymentSchedule.ToList(),
                    RequestStatus = x.RequestStatus,
                    RequestDate = x.RequestDate,
                    Reason = x.Reason,
                    ReviewedBy = x.ReviewedBy,
                    ApproverName = x.Approver.Name,
                    RejectionReason = x.RejectionReason,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return cashAdvanceRequests == null
                ? ApiResponse<object>.Success(null, "Cash advance request not found.")
                : ApiResponse<object>.Success(cashAdvanceRequests, "Data request successful.");
        }
        public async Task<ApiResponse<object>> RequestCashAdvance(CashAdvanceRequestDto request)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Failed(null, "Invalid token.");

            var userId = int.Parse(userIdClaim);

            if (request == null) return ApiResponse<object>.Success(null, "Invalid request.");

            if (request.PaymentDates.Count() != request.MonthsToPay)
                return ApiResponse<object>.Failed("Payment dates must match the number of months to pay.");

            if (request.NeededDate < DateTime.Now)
                return ApiResponse<object>.Failed("Needed date can't be in the past.");

            // Validate that payment dates are not earlier than the NeededDate
            foreach (var paymentDate in request.PaymentDates)
            {
                if (paymentDate < request.NeededDate)
                {
                    return ApiResponse<object>.Failed("Payment dates cannot be earlier than the needed date.");
                }
            }

            var cashAdvanceRequest = new CashAdvanceRequest
            {
                UserId = userId,
                Amount = request.Amount,
                NeededDate = request.NeededDate,
                Reason = request.Reason,
                MonthsToPay = request.MonthsToPay,
            };

            _context.Add(cashAdvanceRequest);
            await _context.SaveChangesAsync();

            if (request.PaymentDates.Count != request.PaymentDates.Distinct().Count()) 
                return ApiResponse<object>.Success(null, "Duplicate payment dates are not allowed.");

            // Calculate equal installment amounts
            decimal monthlyPayment = request.Amount / request.MonthsToPay;

            // Create payment schedule records in one operation
            var paymentSchedules = request.PaymentDates.Select(date => new CashAdvancePaymentSchedule
            {
                CashAdvanceRequestId = cashAdvanceRequest.Id,
                Amount = monthlyPayment,
                PaymentDate = date, // Use the employee-specified date
            }).ToList();

            // Bulk insert in one database operation
            _context.CashAdvancePaymentSchedules.AddRange(paymentSchedules);
            await _context.SaveChangesAsync();

            var notificationMessage = $"{username} has requested a a cash advance with the amount of {cashAdvanceRequest.Amount} on {cashAdvanceRequest.RequestDate:MMM dd, yyyy}.";

            var notification = await _notificationService.CreateAdminNotification(
                title: "New Cash Advance Request",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                createdById: userId,
                type: "Cash Advance Request"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "CashAdvance")
                .Information("{UserName} has requested a cash advance with the amount of {Amount} on {Time}", username, cashAdvanceRequest.Amount, DateTime.Now);

            return ApiResponse<object>.Success(new
            {
                cashAdvanceRequest.Id,
                cashAdvanceRequest.UserId,
                cashAdvanceRequest.Amount,
                cashAdvanceRequest.NeededDate,
                cashAdvanceRequest.MonthsToPay,
                PaymentSchedules = paymentSchedules.Select(ps => new
                {
                    ps.PaymentDate,
                    ps.Amount,
                    Status = ps.Status.ToString() // Include only necessary fields
                }),
                Status = cashAdvanceRequest.Status.ToString(), // Convert Enum to String
                cashAdvanceRequest.RequestDate,
                cashAdvanceRequest.Reason,
            }, "Cash advance request successful.");
        }
        public async Task<ApiResponse<object>> Review(int id, CashAdvanceReview request) //ADD REVIEWED DATE ON THE MODEL
        {
            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminName = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminName) || string.IsNullOrEmpty(adminIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(adminIdClaim);

            var cashAdvanceRequest = await _context.CashAdvanceRequests.FindAsync(id);

            if (cashAdvanceRequest == null) return ApiResponse<object>.Success(null, "Cash advance request not found.");

            var existingSchedules = await _context.CashAdvancePaymentSchedules
                .Where(p => p.CashAdvanceRequestId == id)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();

            if (request.Status == CashAdvanceRequestStatus.Rejected)
            {
                for (int i = 0; i < cashAdvanceRequest.MonthsToPay; i++)
                {
                    existingSchedules[i].Status = CashAdvancePaymentStatus.Rejected;
                    existingSchedules[i].UpdatedAt = DateTime.Now;
                }

                _context.CashAdvancePaymentSchedules.UpdateRange(existingSchedules);

                cashAdvanceRequest.Status = CashAdvanceRequestStatus.Rejected;
                cashAdvanceRequest.RejectionReason = request.RejectionReason;
                cashAdvanceRequest.ReviewedBy = userId;
                cashAdvanceRequest.UpdatedAt = DateTime.Now;
                _context.CashAdvanceRequests.Update(cashAdvanceRequest);

                await _context.SaveChangesAsync();

                return ApiResponse<object>.Success(new
                {
                    cashAdvanceRequest.Id,
                    cashAdvanceRequest.UserId,
                    cashAdvanceRequest.Amount,
                    cashAdvanceRequest.NeededDate,
                    cashAdvanceRequest.MonthsToPay,
                    PaymentSchedules = cashAdvanceRequest.PaymentSchedule.Select(ps => new
                    {
                        ps.PaymentDate,
                        ps.Amount,
                        Status = ps.Status.ToString() // Include only necessary fields
                    }),
                    Status = cashAdvanceRequest.Status.ToString(),
                    cashAdvanceRequest.RequestDate,
                    cashAdvanceRequest.Reason,
                    cashAdvanceRequest.ReviewedBy,
                    cashAdvanceRequest.RejectionReason,
                    cashAdvanceRequest.UpdatedAt
                }, $"Cash advance set to {cashAdvanceRequest.Status}.");
            }

            if (request.Status == CashAdvanceRequestStatus.Approved && request.PaymentDates != null && request.PaymentDates.Count > 0)
            {
                if (request.PaymentDates.Count != cashAdvanceRequest.MonthsToPay)
                    return ApiResponse<object>.Success(null, "The number of provided payment dates must match the required months to pay.");

                if (existingSchedules.Count != cashAdvanceRequest.MonthsToPay)
                    return ApiResponse<object>.Success(null, "Mismatch between stored payment schedules and expected months to pay. Please check the data.");

                // ✅ Update existing schedules, do NOT add new ones
                for (int i = 0; i < cashAdvanceRequest.MonthsToPay; i++)
                {
                    existingSchedules[i].PaymentDate = request.PaymentDates[i];
                    existingSchedules[i].Status = CashAdvancePaymentStatus.ForEmployeeApproval;
                    existingSchedules[i].UpdatedAt = DateTime.Now;
                }

                // ✅ Set status to "ForEmployeeApproval" only if dates were updated
                cashAdvanceRequest.Status = CashAdvanceRequestStatus.ForEmployeeApproval;
            }
            else
            {
                // ✅ Update existing schedules, do NOT add new ones
                for (int i = 0; i < cashAdvanceRequest.MonthsToPay; i++)
                {
                    existingSchedules[i].Status = CashAdvancePaymentStatus.Unpaid;
                    existingSchedules[i].UpdatedAt = DateTime.Now;
                }

                // ✅ If no dates are provided, just approve the request
                cashAdvanceRequest.Status = CashAdvanceRequestStatus.Approved;
                cashAdvanceRequest.UpdatedAt = DateTime.Now;
            }

            cashAdvanceRequest.RejectionReason = request.RejectionReason;
            cashAdvanceRequest.ReviewedBy = userId;
            cashAdvanceRequest.UpdatedAt = DateTime.Now;

            _context.Update(cashAdvanceRequest);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(cashAdvanceRequest.UserId);
            if (user == null) return ApiResponse<object>.Success(null, "User not found.");

            var action = cashAdvanceRequest.Status.ToString();

            var notificationMessage = $"{adminName} has {action} the cash advance request of {user.Name} on {cashAdvanceRequest.UpdatedAt:MMM dd, yyyy}.";
            var employeeNotificationMessage = $"{adminName} has {action} your cash advance request with the amount of {cashAdvanceRequest.Amount} on {DateTime.Now:MMM dd, yyyy}.";

            await _notificationService.CreateAdminNotification(
                title: "Cash Advance Request Update",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                createdById: userId,
                type: "Cash Advance Request"
            );

            await _notificationService.CreateNotification(
            userId: cashAdvanceRequest.UserId,
                title: "Cash Advance Request Update", 
                message: employeeNotificationMessage,
                link: "/api/notification/view/{id}",
                type: "Cash Advance Request"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "CashAdvance")
                .Information("{UserName} has {Action} cash advance request {CashAdvanceId} on {Time}", adminName, action, cashAdvanceRequest.Id, DateTime.Now);

            return ApiResponse<object>.Success(new
            {
                cashAdvanceRequest.Id,
                cashAdvanceRequest.UserId,
                cashAdvanceRequest.Amount,
                cashAdvanceRequest.NeededDate,
                cashAdvanceRequest.MonthsToPay,
                PaymentSchedules = cashAdvanceRequest.PaymentSchedule.Select(ps => new
                {
                    ps.PaymentDate,
                    ps.Amount,
                    Status = ps.Status.ToString() // Include only necessary fields
                }),
                Status = action,
                cashAdvanceRequest.RequestDate,
                cashAdvanceRequest.Reason,
                cashAdvanceRequest.ReviewedBy,
                cashAdvanceRequest.RejectionReason,
                cashAdvanceRequest.UpdatedAt
            }, $"Cash advance set to {action}.");
        }

        public async Task<ApiResponse<object>> Approve(int id, ApproveCashAdvanceDto request)
        {
            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminName = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminName) || string.IsNullOrEmpty(adminIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(adminIdClaim);
            var cashAdvanceRequest = await _context.CashAdvanceRequests.FindAsync(id);

            if (cashAdvanceRequest == null)
                return ApiResponse<object>.Success(null, "Cash advance request not found.");

            if (cashAdvanceRequest.Status != CashAdvanceRequestStatus.Pending)
                return ApiResponse<object>.Success(null, "Can not review a request that has been reviewed already.");

            var existingSchedules = await _context.CashAdvancePaymentSchedules
                .Where(p => p.CashAdvanceRequestId == id)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();

            if (request.PaymentDates != null && request.PaymentDates.Count > 0)
            {
                if (request.PaymentDates.Count != cashAdvanceRequest.MonthsToPay)
                    return ApiResponse<object>.Success(null, "The number of provided payment dates must match the required months to pay.");

                if (existingSchedules.Count != cashAdvanceRequest.MonthsToPay)
                    return ApiResponse<object>.Success(null, "Mismatch between stored payment schedules and expected months to pay. Please check the data.");

                for (int i = 0; i < cashAdvanceRequest.MonthsToPay; i++)
                {
                    existingSchedules[i].PaymentDate = request.PaymentDates[i];
                    existingSchedules[i].Status = CashAdvancePaymentStatus.ForEmployeeApproval;
                    existingSchedules[i].UpdatedAt = DateTime.Now;
                }

                cashAdvanceRequest.Status = CashAdvanceRequestStatus.ForEmployeeApproval;
            }
            else
            {
                for (int i = 0; i < cashAdvanceRequest.MonthsToPay; i++)
                {
                    existingSchedules[i].Status = CashAdvancePaymentStatus.Unpaid;
                    existingSchedules[i].UpdatedAt = DateTime.Now;
                }

                cashAdvanceRequest.Status = CashAdvanceRequestStatus.Approved;
            }

            cashAdvanceRequest.ReviewedBy = userId;
            cashAdvanceRequest.UpdatedAt = DateTime.Now;

            _context.Update(cashAdvanceRequest);
            await _context.SaveChangesAsync();

            return await SendNotificationAndLog(adminName, cashAdvanceRequest, userId);
        }

        public async Task<ApiResponse<object>> Reject(int id, RejectCashAdvanceRequest request)
        {
            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminName = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminName) || string.IsNullOrEmpty(adminIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(adminIdClaim);
            var cashAdvanceRequest = await _context.CashAdvanceRequests.FindAsync(id);

            if (cashAdvanceRequest == null)
                return ApiResponse<object>.Success(null, "Cash advance request not found.");

            if (cashAdvanceRequest.Status != CashAdvanceRequestStatus.Pending)
                return ApiResponse<object>.Success(null, "Can not review a request that has been reviewed already.");

            var existingSchedules = await _context.CashAdvancePaymentSchedules
                .Where(p => p.CashAdvanceRequestId == id)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();

            for (int i = 0; i < cashAdvanceRequest.MonthsToPay; i++)
            {
                existingSchedules[i].Status = CashAdvancePaymentStatus.Rejected;
                existingSchedules[i].UpdatedAt = DateTime.Now;
            }

            _context.CashAdvancePaymentSchedules.UpdateRange(existingSchedules);

            cashAdvanceRequest.Status = CashAdvanceRequestStatus.Rejected;
            cashAdvanceRequest.RejectionReason = request.RejectionReason;
            cashAdvanceRequest.ReviewedBy = userId;
            cashAdvanceRequest.UpdatedAt = DateTime.Now;

            _context.CashAdvanceRequests.Update(cashAdvanceRequest);
            await _context.SaveChangesAsync();

            return await SendNotificationAndLog(adminName, cashAdvanceRequest, userId);
        }

        public async Task<ApiResponse<object>> UpdatePaymentStatus(int id, UpdatePaymentStatusDto request)
        {
            var admin = _httpContextAccessor.HttpContext?.User;
            var adminIdClaim = admin?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminName = admin?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(adminName) || string.IsNullOrEmpty(adminIdClaim))
                return ApiResponse<object>.Failed("Invalid token.");

            var userId = int.Parse(adminIdClaim);

            var cashAdvancePaymentSched = await _context.CashAdvancePaymentSchedules.FindAsync(id);

            if (cashAdvancePaymentSched.ImageFilePath == null)
                return ApiResponse<object>.Failed("Receipt is required before updating payment status.");

            if (cashAdvancePaymentSched == null)
                return ApiResponse<object>.Failed("Cash advance payment schedule not found.");

            var cashAdvanceRequest = await _context.CashAdvanceRequests.FindAsync(cashAdvancePaymentSched.CashAdvanceRequestId);
            if (cashAdvanceRequest == null)
                return ApiResponse<object>.Failed("Cash advance request not found.");

            if (cashAdvancePaymentSched.Status != CashAdvancePaymentStatus.Unpaid && cashAdvancePaymentSched.Status != CashAdvancePaymentStatus.Paid)
                return ApiResponse<object>.Failed("Cannot update payment status.");

            var user = await _context.Users.FindAsync(cashAdvanceRequest.UserId);

            // **Update Payment Schedule Status**
            cashAdvancePaymentSched.Status = request.Status;
            cashAdvancePaymentSched.UpdatedAt = DateTime.Now;

            _context.CashAdvancePaymentSchedules.Update(cashAdvancePaymentSched);
            await _context.SaveChangesAsync();

            // **Check if all related payment schedules are Paid**
            bool allPaid = await _context.CashAdvancePaymentSchedules
                .Where(p => p.CashAdvanceRequestId == cashAdvanceRequest.Id)
                .AllAsync(p => p.Status == CashAdvancePaymentStatus.Paid);

            if (allPaid)
            {
                cashAdvanceRequest.Status = CashAdvanceRequestStatus.Paid; // ✅ Update CashAdvanceRequestStatus, NOT Status
                _context.CashAdvanceRequests.Update(cashAdvanceRequest);
                await _context.SaveChangesAsync();
            }

            // **Notifications**
            var action = cashAdvancePaymentSched.Status.ToString();
            var notificationMessage = $"{adminName} has updated the cash advance request payment of {user.Name} to {action} on {cashAdvancePaymentSched.UpdatedAt:MMM dd, yyyy}.";
            var employeeNotificationMessage = $"{adminName} has updated your cash advance request with the amount of {cashAdvancePaymentSched.Amount} to {action} on {DateTime.Now:MMM dd, yyyy}.";

            await _notificationService.CreateAdminNotification(
                title: "Cash Advance Request Update",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                createdById: userId,
                type: "Cash Advance Request"
            );

            await _notificationService.CreateNotification(
                userId: cashAdvanceRequest.UserId,
                title: "Cash Advance Request Update",
                message: employeeNotificationMessage,
                link: "/api/notification/view/{id}",
                type: "Cash Advance Request"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "CashAdvance")
                .Information("{UserName} has set {CashAdvanceId} to {Action} on {Time}", adminName, cashAdvancePaymentSched.Id, action, DateTime.Now);

            return ApiResponse<object>.Success(cashAdvancePaymentSched, "Payment schedule updated.");
        }

        private async Task<ApiResponse<object>> SendNotificationAndLog(string adminName, CashAdvanceRequest cashAdvanceRequest, int userId)
        {
            var user = await _context.Users.FindAsync(cashAdvanceRequest.UserId);
            if (user == null) return ApiResponse<object>.Success(null, "User not found.");

            var action = cashAdvanceRequest.Status.ToString();
            var notificationMessage = $"{adminName} has {action} the cash advance request of {user.Name} on {cashAdvanceRequest.UpdatedAt:MMM dd, yyyy}.";
            var employeeNotificationMessage = $"{adminName} has {action} your cash advance request with the amount of {cashAdvanceRequest.Amount} on {DateTime.Now:MMM dd, yyyy}.";

            await _notificationService.CreateAdminNotification(
                title: "Cash Advance Request Update",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                createdById: userId,
                type: "Cash Advance Request"
            );

            await _notificationService.CreateNotification(
                userId: cashAdvanceRequest.UserId,
                title: "Cash Advance Request Update",
                message: employeeNotificationMessage,
                link: "/api/notification/view/{id}",
                type: "Cash Advance Request"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "CashAdvance")
                .Information("{UserName} has {Action} cash advance request {CashAdvanceId} on {Time}", adminName, action, cashAdvanceRequest.Id, DateTime.Now);

            return ApiResponse<object>.Success(null, $"Cash advance set to {action}.");
        }

        public async Task<ApiResponse<object>> EmployeeReview(int id, EmployeeCashAdvanceReview request) //ADD REVIEWED DATE ON THE MODEL
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(userIdClaim);

            var userContext = await _context.Users.FindAsync(userId);

            var cashAdvanceRequest = await _context.CashAdvanceRequests
                .Include(ca => ca.PaymentSchedule) // Ensure schedules are loaded
                .FirstOrDefaultAsync(ca => ca.Id == id);

            var existingSchedules = await _context.CashAdvancePaymentSchedules
                .Where(p => p.CashAdvanceRequestId == id)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();

            if (cashAdvanceRequest == null) return ApiResponse<object>.Success(null, "Cash advance request not found.");

            if (cashAdvanceRequest.Status != CashAdvanceRequestStatus.ForEmployeeApproval)
                return ApiResponse<object>.Success(null, "This cash advance request is not for employee review.");

            if (userId != cashAdvanceRequest.UserId)
                return ApiResponse<object>.Success(null, "You are not authorized to review this request.");

            if (request.Status == CashAdvanceRequestStatus.Rejected)
            {
                if (existingSchedules != null)
                {
                    foreach (var schedule in existingSchedules)
                    {
                        schedule.Status = CashAdvancePaymentStatus.Rejected;
                    }
                }

                cashAdvanceRequest.Status = CashAdvanceRequestStatus.Rejected;
                cashAdvanceRequest.UpdatedAt = DateTime.Now;

                _context.CashAdvanceRequests.Update(cashAdvanceRequest);
                await _context.SaveChangesAsync();

                return ApiResponse<object>.Success(new
                {
                    cashAdvanceRequest.Id,
                    cashAdvanceRequest.UserId,
                    cashAdvanceRequest.Amount,
                    cashAdvanceRequest.NeededDate,
                    cashAdvanceRequest.MonthsToPay,
                    PaymentSchedules = cashAdvanceRequest.PaymentSchedule.Select(ps => new
                    {
                        ps.PaymentDate,
                        ps.Amount,
                        Status = ps.Status.ToString() // Include only necessary fields
                    }),
                    Status = cashAdvanceRequest.Status.ToString(),
                    cashAdvanceRequest.RequestDate,
                    cashAdvanceRequest.Reason,
                    cashAdvanceRequest.ReviewedBy,
                    cashAdvanceRequest.RejectionReason,
                    cashAdvanceRequest.UpdatedAt
                }, $"Cash advance set to {cashAdvanceRequest.Status}.");
            }

            if (request.Status == CashAdvanceRequestStatus.Approved)
            {
                if (existingSchedules != null)
                {
                    foreach (var schedule in existingSchedules)
                    {
                        schedule.Status = CashAdvancePaymentStatus.Unpaid;
                    }
                }

                cashAdvanceRequest.Status = CashAdvanceRequestStatus.Approved;
                cashAdvanceRequest.UpdatedAt = DateTime.Now;
            }

            cashAdvanceRequest.UpdatedAt = DateTime.Now;

            _context.Update(cashAdvanceRequest);
            await _context.SaveChangesAsync();

            if (userContext == null) return ApiResponse<object>.Success(null, "User not found.");

            var action = request.Status.ToString();

            var notificationMessage = $"{username} has {action} the changes to their cash advance request.";

            await _notificationService.CreateAdminNotification(
                title: $"Cash Advance Request {action} by {username}",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                createdById: userId,
                type: "Cash Advance Request"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "CashAdvance")
                .Information("{UserName} has {Action} the payment schedule changes of {CashAdvanceId} on {Time}", username, action, cashAdvanceRequest.Id, DateTime.Now);

            return ApiResponse<object>.Success(new
            {
                cashAdvanceRequest.Id,
                cashAdvanceRequest.UserId,
                cashAdvanceRequest.Amount,
                cashAdvanceRequest.NeededDate,
                cashAdvanceRequest.MonthsToPay,
                PaymentSchedules = cashAdvanceRequest.PaymentSchedule.Select(ps => new
                {
                    ps.PaymentDate,
                    ps.Amount,
                    Status = ps.Status.ToString() // Include only necessary fields
                }),
                Status = action,
                cashAdvanceRequest.RequestDate,
                cashAdvanceRequest.Reason,
                cashAdvanceRequest.ReviewedBy,
                cashAdvanceRequest.RejectionReason,
                cashAdvanceRequest.UpdatedAt
            }, $"Cash advance set to {action}.");
        }

        public async Task<ApiResponse<object>> UploadReceipt(int id, [FromForm] IFormFile receiptImage) //ADD REVIEWED DATE ON THE MODEL
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                return ApiResponse<object>.Success(null, "Invalid token.");

            var userId = int.Parse(userIdClaim);

            var userContext = await _context.Users.FindAsync(userId);

            if (receiptImage == null || receiptImage.Length == 0)
                            return ApiResponse<object>.Success(null, "You are not authorized to review this request.");

            // ✅ Define allowed file types
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var extension = Path.GetExtension(receiptImage.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return ApiResponse<object>.Failed("Invalid file type. Allowed formats: JPG, PNG, PDF.");

            // ✅ Limit file size to 2MB
            var maxSize = 2 * 1024 * 1024; // 2MB
            if (receiptImage.Length > maxSize)
                return ApiResponse<object>.Failed("File size exceeds 2MB limit.");


            // Find the existing payment schedule record.
            var schedule = await _context.CashAdvancePaymentSchedules.FindAsync(id);

            if (schedule.Status != CashAdvancePaymentStatus.Unpaid)
                return ApiResponse<object>.Failed("File upload is only available for unpaid payment schedules.");

            if (schedule.ImageFilePath is not null)
                return ApiResponse<object>.Failed("Can't upload file because a file already exist for this payment.");

            if (schedule == null)
                return ApiResponse<object>.Failed("Payment schedule record not found.");

            // Define the folder to store uploaded files.
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate a unique file name to avoid naming conflicts.
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(receiptImage.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file on disk.
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await receiptImage.CopyToAsync(stream);
            }

            // Update the payment schedule record with the file path.
            schedule.ImageFilePath = uniqueFileName;
            schedule.UpdatedAt = DateTime.Now;

            _context.CashAdvancePaymentSchedules.Update(schedule);
            await _context.SaveChangesAsync();

            var notificationMessage = $"{username} has uploaded a receipt for his/her cash advance payment.";

            await _notificationService.CreateAdminNotification(
                title: $"Payment Receipt Upload by {username}",
                message: notificationMessage,
                link: "/api/notification/view/{id}",
                createdById: userId,
                type: "Cash Advance Request"
            );

            Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                .ForContext("Type", "CashAdvance")
                .Information("{username} has uploaded a receipt for his/her cash advance payment on {Time}.", username, DateTime.Now);

            return ApiResponse<object>.Success(schedule, $"Receipt upload successful.");
        }
    }
}
