﻿using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;

namespace AttendanceTracker1.Services.LeaveService
{
    public interface ILeaveService
    {
        public Task<ApiResponse<object>> GetLeaveRequests(int page, int pageSize);
        public Task<ApiResponse<object>> GetLeaveRequestById(int id);
        public Task<ApiResponse<object>> GetLeaveRequestByUserId(int id);
        public Task<ApiResponse<object>> GetSelfLeaveRequest(int page, int pageSize);
        public Task<ApiResponse<object>> RequestLeave(RequestLeaveDto request);
        public Task<ApiResponse<object>> Review(int id, LeaveReviewDto request);
        public Task<ApiResponse<object>> Approve(int id);
        public Task<ApiResponse<object>> Reject(int id, LeaveRejectDto request);
        public Task<ApiResponse<object>> UpdateLeaveRequest(int id, UpdateLeaveDto request);
        public Task<ApiResponse<object>> CancelLeaveRequest(int id);
    }
}
