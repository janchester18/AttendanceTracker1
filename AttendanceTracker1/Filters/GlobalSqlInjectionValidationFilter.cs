using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AttendanceTracker1.Filters
{
    public class GlobalSqlInjectionValidationFilter : IActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GlobalSqlInjectionValidationFilter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Adjust this regex as needed for your security requirements
        private static readonly Regex SqlInjectionRegex = new Regex(@"([;<>]+|(--)+)", RegexOptions.Compiled);

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            var username = user?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

            foreach (var arg in context.ActionArguments)
            {
                if (ContainsSqlInjection(arg.Value))
                {

                    context.Result = new ObjectResult(ApiResponse<object>.Success(null, "Input contains invalid characters."))
                    {
                        StatusCode = StatusCodes.Status200OK
                    };

                    var suspiciousInput = arg.Value != null ? JsonSerializer.Serialize(arg.Value) : "Unknown";

                    var logEntry = new
                    {
                        UserName = username,
                        UserId = userId,
                        IPAddress = ipAddress,
                        ParameterName = arg.Key,
                        Time = DateTime.Now
                    };

                    Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                        .ForContext("Type", "Suspicious Behavior")
                        .Information("SQL Injection attempt detected! UserName: {UserName}, UserId: {UserId}, IPAddress: {IPAddress}, Timestamp: {Timestamp}",
                            username, userId, ipAddress, DateTime.Now);

                    return;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No post-processing required
        }

        private bool ContainsSqlInjection(object obj)
        {
            if (obj == null)
                return false;

            if (obj is string str)
            {
                return SqlInjectionRegex.IsMatch(str);
            }

            // For complex objects, inspect their public string properties
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(string))
                {
                    // Skip validation for the "Body" property of EmailRequestDto
                    if (obj is EmailRequestDto && prop.Name == "Body")
                    {
                        continue;
                    }

                    var value = prop.GetValue(obj) as string;
                    if (!string.IsNullOrEmpty(value) && SqlInjectionRegex.IsMatch(value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}