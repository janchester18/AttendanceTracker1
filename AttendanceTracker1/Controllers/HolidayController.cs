using System.Security.Claims;
using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidayController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HolidayController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> GetHolidays (int page = 1,  int pageSize = 10)
        {
            try
            {
                var skip = (page - 1) * pageSize;

                var totalRecords = await _context.Holidays.CountAsync();

                var holidays = await _context.Holidays
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                var response = ApiResponse<object>.Success(new
                {
                    holidays = holidays,
                    totalRecords,
                    totalPages,
                    currentPage = page,
                    pageSize,
                    hasNextPage = page < totalPages,
                    hasPreviousPage = page > 1
                });

                return Ok(response);
            }
            catch (Exception ex) 
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddHoliday([FromBody] AddHolidayDto request)
        {
            try
            {
                var holiday = new Holiday
                {
                    Name = request.Name,
                    Date = request.Date,
                    IsPaid = request.IsPaid,
                    IsNational = request.IsNational,
                    Type = request.Type,
                    UpdatedAt = DateTime.Now
                };

                _context.Holidays.Add(holiday);
                await _context.SaveChangesAsync();

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                var userId = int.Parse(userIdClaim);

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                   .ForContext("Type", "Holiday")
                   .Information("{UserName} has added holiday {Id} at {Time}", username, holiday.Id, DateTime.Now);

                return Ok(ApiResponse<object>.Success(new
                {
                    message = $"Holiday successfully created."
                }));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditHoliday(int id, [FromBody] EditHolidayDto request)
        {
            try
            {
                var holiday = await _context.Holidays.FirstOrDefaultAsync(h => h.Id == id);

                if (holiday == null)
                {
                    return BadRequest("Holiday not found.");
                }

                holiday.Name = request.Name ?? holiday.Name;
                holiday.Date = request.Date ?? holiday.Date;
                holiday.IsPaid = request.IsPaid ?? holiday.IsPaid;
                holiday.IsNational = request.IsNational ?? holiday.IsNational;
                holiday.Type = request.Type ?? holiday.Type;
                holiday.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("Invalid token.");
                }

                var userId = int.Parse(userIdClaim);

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                   .ForContext("Type", "Holiday")
                   .Information("{UserName} has edited holiday {Id} at {Time}", username, holiday.Id, DateTime.Now);

                var response = new
                {
                    name = holiday.Name,
                    date = holiday.Date,
                    isPaid = holiday.IsPaid,
                    isNational = holiday.IsNational,
                    type = holiday.Type,
                    updatedAt = holiday.UpdatedAt
                };

                return Ok(ApiResponse<object>.Success(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Failed(ex.Message));
            }
        }
    }
}
