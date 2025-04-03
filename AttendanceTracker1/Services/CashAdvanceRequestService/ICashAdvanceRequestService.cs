using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Services.CashAdvanceRequestService
{
    public interface ICashAdvanceRequestService
    {
        public Task<ApiResponse<object>> GetCashAdvanceRequests(int page, int pageSize, string keyword, DateTime? startDate, DateTime? endDate);
        public Task<ApiResponse<object>> GetSelfCashAdvanceRequests(int page, int pageSize);
        public Task<ApiResponse<object>> GetCashAdvanceRequestById(int id);
        public Task<ApiResponse<object>> GetCashAdvanceRequestByUserId(int id);
        public Task<ApiResponse<object>> RequestCashAdvance(CashAdvanceRequestDto cashAdvanceRequest);
        public Task<ApiResponse<object>> Review(int id, CashAdvanceReview request);
        public Task<ApiResponse<object>> Approve(int id, ApproveCashAdvanceDto request);
        public Task<ApiResponse<object>> Reject(int id, RejectCashAdvanceRequest request);
        public Task<ApiResponse<object>> UpdatePaymentStatus(int id, UpdatePaymentStatusDto request);
        public Task<ApiResponse<object>> EmployeeReview(int id, EmployeeCashAdvanceReview request);
        public Task<ApiResponse<object>> UploadReceipt(int id, [FromForm] IFormFile receiptImage);
    }
}
