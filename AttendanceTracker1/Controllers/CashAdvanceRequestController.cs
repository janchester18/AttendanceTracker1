using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services.CashAdvanceRequestService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CashAdvanceRequestController : ControllerBase
    {
        private readonly ICashAdvanceRequestService _cashAdvanceRequestService;

        public CashAdvanceRequestController(ICashAdvanceRequestService cashAdvanceRequestService)
        {
            _cashAdvanceRequestService = cashAdvanceRequestService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCashAdvanceRequests (int page = 1, int pageSize = 10, string keyword = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var response = await _cashAdvanceRequestService.GetCashAdvanceRequests(page, pageSize, keyword, startDate, endDate);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("self")]
        [Authorize]
        public async Task<IActionResult> GetSelfCashAdvanceRequests(int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _cashAdvanceRequestService.GetSelfCashAdvanceRequests(page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetCashAdvanceRequestsById (int id)
        {
            try
            {
                var response = await _cashAdvanceRequestService.GetCashAdvanceRequestById(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IActionResult> GetCashAdvanceRequestsByUserId(int id)
        {
            try
            {
                var response = await _cashAdvanceRequestService.GetCashAdvanceRequestByUserId(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestCashAdvance([FromBody] CashAdvanceRequestDto cashAdvanceRequestDto)
        {
            try
            {
                var response = await _cashAdvanceRequestService.RequestCashAdvance(cashAdvanceRequestDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }

        [HttpPut("review/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Review(int id, [FromBody] CashAdvanceReview cashAdvanceReview)
        {
            try
            {
                var response = await _cashAdvanceRequestService.Review(id, cashAdvanceReview);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));

            }
        }

        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, [FromBody] ApproveCashAdvanceDto cashAdvanceReview)
        {
            try
            {
                var response = await _cashAdvanceRequestService.Approve(id, cashAdvanceReview);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));

            }
        }

        [HttpPut("reject/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectCashAdvanceRequest cashAdvanceReview)
        {
            try
            {
                var response = await _cashAdvanceRequestService.Reject(id, cashAdvanceReview);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));

            }
        }

        [HttpPut("update-status/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDto request)
        {
            try
            {
                var response = await _cashAdvanceRequestService.UpdatePaymentStatus(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));

            }
        }

        [HttpPut("employee-review/{id}")]
        [Authorize]
        public async Task<IActionResult> EmployeeReview(int id, [FromBody] EmployeeCashAdvanceReview request)
        {
            try
            {
                var response = await _cashAdvanceRequestService.EmployeeReview(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);

            }
        }

        [HttpPut("upload-receipt/{id}")]
        [Authorize]
        public async Task<IActionResult> UploadReceipt(int id, [FromForm] IFormFile receiptImage)
        {
            try
            {
                var response = await _cashAdvanceRequestService.UploadReceipt(id, receiptImage);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);

            }
        }
    }
}
