using AttendanceTracker1.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AttendanceTracker1.Filters
{
    public class GlobalSqlInjectionValidationFilter : IActionFilter
    {
        // Adjust this regex as needed for your security requirements
        private static readonly Regex SqlInjectionRegex = new Regex(@"([';<>]+|(--)+)", RegexOptions.Compiled);

        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var arg in context.ActionArguments.Values)
            {
                if (ContainsSqlInjection(arg))
                {
                    context.Result = new BadRequestObjectResult("Input contains invalid characters.");
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