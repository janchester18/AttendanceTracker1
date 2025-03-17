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
        [Authorize]
        public async Task<IActionResult> GetCashAdvanceRequests (int page = 1, int pageSize = 10)
        {
            try
            {
                var response = await _cashAdvanceRequestService.GetCashAdvanceRequests(page, pageSize);
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
        [Authorize]
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
    }
}
