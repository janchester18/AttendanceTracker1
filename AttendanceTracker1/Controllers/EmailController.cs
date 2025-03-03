using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequestDto request)
        {
            try
            {
                await _emailService.SendEmailAsync(request.To, request.Subject, request.Body);
                return Ok(ApiResponse<object>.Success(null, "Email sent successfully!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
