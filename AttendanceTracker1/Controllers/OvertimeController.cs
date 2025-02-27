﻿using System.Security.Claims;
using System.Text.RegularExpressions;
using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OvertimeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public OvertimeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOvertimeRequests(int page = 1, int pageSize = 10)
        {
            try
            {
                var skip = (page - 1) * pageSize;

                var totalRecords = await _context.Overtimes.CountAsync();

                // Fetch the overtime requests with pagination
                var overtimes = await _context.Overtimes
                    .Include(o => o.User)
                    .Include(o => o.Approver)
                    .OrderBy(o => o.CreatedAt) // Stable ordering
                    .Skip(skip) // Skip the records for the previous pages
                    .Take(pageSize) // Limit the number of records to the page size
                    .Select(o => new OvertimeResponseDto
                    {
                        Id = o.Id,
                        UserId = o.UserId,
                        EmployeeName = o.User != null ? o.User.Name : "Unknown",
                        Date = o.Date,
                        StartTime = o.StartTime,
                        EndTime = o.EndTime,
                        Reason = o.Reason,
                        ExpectedOutput = o.ExpectedOutput,
                        Status = o.Status.ToString(),
                        ReviewedBy = o.ReviewedBy,
                        ApproverName = o.Approver != null ? o.Approver.Name : null,
                        RejectionReason = o.RejectionReason,
                        CreatedAt = o.CreatedAt,
                        UpdatedAt = o.UpdatedAt
                    })
                    .ToListAsync();

                // Calculate total pages
                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var response = ApiResponse<object>.Success(new
                {
                    overtimes,
                    totalRecords,
                    totalPages,
                    currentPage = page,
                    pageSize,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                }, "Overtime data request successful.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOvertimeRequestById(int id)
        {
            try
            {
                var overtime = await _context.Overtimes
                .Where(o => o.Id == id)
                .Include(o => o.User)
                .Include(o => o.Approver)
                .Select(o => new OvertimeResponseDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    EmployeeName = o.User != null ? o.User.Name : "Unknown",
                    Date = o.Date,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    Reason = o.Reason,
                    ExpectedOutput = o.ExpectedOutput,
                    Status = o.Status.ToString(),
                    ReviewedBy = o.ReviewedBy,
                    ApproverName = o.Approver != null ? o.Approver.Name : null,
                    RejectionReason = o.RejectionReason,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .FirstOrDefaultAsync();

                if (overtime == null)
                {
                    return NotFound(ApiResponse<object>.Failed("Overtime request not found."));
                }

                return Ok(ApiResponse<object>.Success(overtime, "Overtime data request successful."));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetOvertimeRequestByUserId(int id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if(user == null)
                {
                    return Ok(ApiResponse<object>.Failed("User not found."));
                }

                var overtime = await _context.Overtimes
                .Where(o => o.UserId == id)
                .Include(o => o.User)
                .Include(o => o.Approver)
                .Select(o => new OvertimeResponseDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    EmployeeName = o.User != null ? o.User.Name : "Unknown",
                    Date = o.Date,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    Reason = o.Reason,
                    ExpectedOutput = o.ExpectedOutput,
                    Status = o.Status.ToString(),
                    ReviewedBy = o.ReviewedBy,
                    ApproverName = o.Approver != null ? o.Approver.Name : null,
                    RejectionReason = o.RejectionReason,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .ToListAsync();

                return Ok(ApiResponse<object>.Success(overtime, "Overtime data request successful."));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestOvertime([FromBody] OvertimeRequestDto overtimeRequest)
        {
            try
            {
                if (overtimeRequest.StartTime >= overtimeRequest.EndTime)
                {
                    return Ok(ApiResponse<object>.Failed("Start time must be before end time."));
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Ok(ApiResponse<object>.Failed("Invalid token."));
                }

                var userId = int.Parse(userIdClaim);

                // 🔹 Create Overtime Request
                var overtime = new Overtime
                {
                    UserId = userId,
                    Date = overtimeRequest.Date,
                    StartTime = overtimeRequest.StartTime,
                    EndTime = overtimeRequest.EndTime,
                    Reason = overtimeRequest.Reason,
                    ExpectedOutput = overtimeRequest.ExpectedOutput,
                    Status = OvertimeRequestStatus.Pending,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Overtimes.Add(overtime);
                await _context.SaveChangesAsync();

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Overtime")
                    .Information("{UserName} has requested an overtime at {Time}", username, DateTime.Now);

                return Ok(ApiResponse<object>.Success(new 
                    { OvertimeId = overtime.Id }, "Overtime request submitted successfully."));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("review/{id}")]
        [Authorize (Roles="Admin")]
        public async Task<IActionResult> Review(int id, [FromBody] OvertimeReview request)
        {
            try
            {
                var overtime = await _context.Overtimes.FirstOrDefaultAsync(o => o.Id == id);
                if (overtime == null)
                {
                    return Ok(ApiResponse<object>.Failed("User not found."));
                }

                // ✅ Validate if status is a valid enum value
                if (!Enum.IsDefined(typeof(OvertimeRequestStatus), request.Status))
                {
                    return Ok(ApiResponse<object>.Failed("Invalid leave status."));
                }

                // Check if RejectionReason is provided when status is Rejected
                if (request.Status == OvertimeRequestStatus.Rejected &&
                    string.IsNullOrWhiteSpace(request.RejectionReason))
                {
                    return Ok(ApiResponse<object>.Failed("Rejection reason is required when status is Rejected."));
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Ok(ApiResponse<object>.Failed("Invalid token."));
                }

                var userId = int.Parse(userIdClaim);

                overtime.Status = request.Status;
                overtime.ReviewedBy = userId;
                overtime.RejectionReason = request.RejectionReason;

                await _context.SaveChangesAsync();

                var action = overtime.Status.ToString();

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Overtime")
                    .Information("{UserName} has {Action} overtime {Id} at {Time}", username, action, id, DateTime.Now);

                return Ok(ApiResponse<object>.Success(new 
                    { message = $"Overtime request {id} has been {overtime.Status}." 
                }));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
