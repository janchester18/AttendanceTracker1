using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using AttendanceTracker1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AttendanceTracker1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly TokenBlacklistService _tokenBlacklistService;
        public AuthController(ApplicationDbContext context, 
            IConfiguration config, 
            TokenBlacklistService tokenBlacklistService)
        {
            _context = context;
            _config = config;
            _tokenBlacklistService = tokenBlacklistService;
        }

        [HttpPost("add-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    return Ok(ApiResponse<object>.Success(null, "Email is already registered."));
                }

                var user = new User
                {
                    Id = 0,
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Role = model.Role ?? "Employee",
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                };

                user.SetPassword(model.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var username = user.Name;
                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Authorization")
                    .Information("{UserName} has been registered at {Time}", username, DateTime.Now);

                return Ok(ApiResponse<object>.Success(new { user.Name, user.Role }, "User registered successfully."));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }

        //Login User
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null || !user.VerifyPassword(model.Password))
                    return Ok(ApiResponse<object>.Success(null, "Invalid credentials.")); //verify

                var role = user.Role;
                var accessToken = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Save the refresh token in an HttpOnly secure cookie
                SetRefreshTokenCookie(refreshToken);

                var response = ApiResponse<object>.Success(new
                {
                    accessToken,
                    refreshToken,
                    role,
                }, "Login successful");

                var username = user?.Name ?? "Unknown";

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Authorization")
                    .Information("{UserName} has logged in at {Time}", username, DateTime.Now);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get the user ID from the JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Ok(ApiResponse<object>.Success(null, "Invalid token."));
                }

                var userId = int.Parse(userIdClaim);

                // Retrieve the user from the database to get the username.
                var user = await _context.Users.FindAsync(userId);
                var username = user?.Name ?? "Unknown";

                // Get JWT token from the Authorization header
                string? token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                    if (jwtToken != null)
                    {
                        var expiry = jwtToken.ValidTo;
                        _tokenBlacklistService.AddToBlacklist(token, expiry); // Add token to blacklist
                    }
                }

                Response.Cookies.Delete("refreshToken");

                // not necessary but just to make sure when the frontend does not remove the token from the header, the backend will do it.
                Response.Headers.Append("Clear-Token", "true");

                Serilog.Log.ForContext("SourceContext", "AttendanceTracker")
                    .ForContext("Type", "Authorization")
                    .Information("{UserName} has logged out at {Time}", username, DateTime.Now);

                return Ok(ApiResponse<object>.Success(null, "Logged out successfully"));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequestDto refreshRequest)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(refreshRequest.AccessToken);
                if (principal == null)
                {
                    return Ok(ApiResponse<object>.Success(null, "Invalid token"));
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Ok(ApiResponse<object>.Success(null, "Invalid user ID" ));
                }

                // Retrieve refresh token from the HttpOnly cookie
                var refreshTokenFromCookie = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshTokenFromCookie))
                {
                    return Ok(ApiResponse<object>.Success(null, "Refresh token missing" ));
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    return Ok(ApiResponse<object>.Success(null, "Invalid user ID format" ));
                }

                var user = _context.Users.Find(userIdInt);
                if (user == null) return Ok(ApiResponse<object>.Success(null, "User not found" ));

                // Issue a new access token
                var newAccessToken = GenerateJwtToken(user);

                return Ok(ApiResponse<object>.Success(new 
                    { accessToken = newAccessToken 
                }));
            }
            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Failed(ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }

        //Generate JWT Token
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //verify
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"])), 
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Generate Refresh Token
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // 🔹 Extract Claims from Expired Token
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false // Ignore expiry for extracting claims
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        // ✅ Store Refresh Token in HttpOnly Secure Cookie
        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,  // Prevent JavaScript access (XSS protection)
                Secure = true,    // Send only over HTTPS
                SameSite = SameSiteMode.Strict, // Restrict cross-site requests
                Expires = DateTime.UtcNow.AddDays(7) // Set expiry as needed
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    }
}