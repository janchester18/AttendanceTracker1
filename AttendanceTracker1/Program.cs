using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AttendanceTracker1.Data;
using AttendanceTracker1.Filters;
using AttendanceTracker1.Middlewares;
using AttendanceTracker1.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using Serilog.Events;
using System.Collections.ObjectModel;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add CORS (For frontend access)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 🔹 Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalSqlInjectionValidationFilter>();
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// 🔹 Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings.GetValue<string>("Key") ?? throw new Exception("JWT Key is missing"));

// In-memory blacklist (Replace with database or Redis for production)
var blacklistedTokens = new ConcurrentDictionary<string, DateTime>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Optional: Allow JWT in Cookies
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["AuthToken"]; // Read token from cookies
                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                var tokenBlacklistService = context.HttpContext.RequestServices.GetRequiredService<TokenBlacklistService>();
                var token = context.SecurityToken as JwtSecurityToken;

                if (token != null && tokenBlacklistService.IsTokenBlacklisted(token.RawData))
                {
                    context.Fail("Unauthorized: Token is blacklisted.");
                }
            }
        };
    });

// 🔹 Enable Role-Based Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
});

builder.Services.AddSingleton<TokenBlacklistService>();

// Define the SQL Server connection string and table name
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var tableName = "Logs";

var additionalColumns = new Collection<SqlColumn>
{
    new SqlColumn
    {
        ColumnName = "Type",
        DataType = SqlDbType.NVarChar,
        DataLength = 50,   // Adjust length as needed
        AllowNull = true
    }
};

var columnOptions = new ColumnOptions
{
    AdditionalColumns = additionalColumns
};

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    // Global minimum level: capture everything from Debug upward
    .MinimumLevel.Debug()
    // Override Microsoft logs generally to Warning...
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    // ...but allow host lifecycle logs (like "Now listening on...") at Information
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    // Also override System logs if needed
    .MinimumLevel.Override("System", LogEventLevel.Warning)

    // Console sink: show everything from Debug level
    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)

    // Sub-logger for the database sink:
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e =>
            e.Level >= LogEventLevel.Information &&
            // Only include logs that originate from your custom namespace
            e.Properties.TryGetValue("SourceContext", out var source) &&
            source.ToString().Contains("AttendanceTracker")
        )
        .WriteTo.MSSqlServer(
            connectionString: connectionString,
            sinkOptions: new MSSqlServerSinkOptions { TableName = tableName, AutoCreateSqlTable = true },
            columnOptions: columnOptions,
            restrictedToMinimumLevel: LogEventLevel.Information
        )
    )
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);

var app = builder.Build();

// 🔹 Configure Middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll"); // 🔹 Enable CORS

app.UseAuthentication();
app.Use(async (context, next) =>
{
    var tokenBlacklistService = context.RequestServices.GetRequiredService<TokenBlacklistService>();

    // Get JWT token from Authorization header
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

    if (!string.IsNullOrEmpty(token) && tokenBlacklistService.IsTokenBlacklisted(token))
    {
        context.Response.StatusCode = 401; // Unauthorized
        await context.Response.WriteAsync("Unauthorized: Token is blacklisted.");
        return;
    }

    await next();
});
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.MapControllers();

app.UseSerilogRequestLogging();

app.Run();
