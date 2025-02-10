using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AttendanceTracker1.Data;
using AttendanceTracker1.DTO;
using AttendanceTracker1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
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

        private static Dictionary<string, string> refreshTokens = new();

        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email is already registered." });
            }

            var user = new User
            {
                Id = 0, 
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Role = model.Role,
                Created = DateTime.Now,
                Updated = DateTime.Now
            };

            user.SetPassword(model.Password); 

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        //Login User
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 🔹 Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null || !user.VerifyPassword(model.Password))
                return Unauthorized(new { message = "Invalid credentials" });

            // 🔥 Generate Tokens
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // 🔥 Store refresh token in memory (Can store in DB)
            refreshTokens[user.Id.ToString()] = refreshToken;

            return Ok(new
            {
                message = "Login successful",
                accessToken,
                refreshToken,
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role
                }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutRequest logoutRequest)
        {
            var principal = GetPrincipalFromExpiredToken(logoutRequest.AccessToken);
            if (principal == null)
                return Unauthorized(new { message = "Invalid token" });

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null && refreshTokens.ContainsKey(userId))
            {
                refreshTokens.Remove(userId); 
            }

            return Ok(new { message = "Logged out successfully" });
        }

        //Refresh Token Endpoint
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequestDto refreshRequest)
        {
            var principal = GetPrincipalFromExpiredToken(refreshRequest.AccessToken);
            if (principal == null) return Unauthorized(new { message = "Invalid token" });

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !refreshTokens.ContainsKey(userId) || refreshTokens[userId] != refreshRequest.RefreshToken)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            var user = _context.Users.Find(userId);
            if (user == null) return Unauthorized(new { message = "User not found" });

            var newAccessToken = GenerateJwtToken(user);
            return Ok(new { accessToken = newAccessToken });
        }

        //Generate JWT Token
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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

    }
}
