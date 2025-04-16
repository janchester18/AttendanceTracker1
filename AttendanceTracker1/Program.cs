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
using Microsoft.AspNetCore.Mvc;
using AttendanceTracker1.Services.AttendanceService;
using AttendanceTracker1.Services.EmailService;
using AttendanceTracker1.Services.HolidayService;
using AttendanceTracker1.Services.LeaveService;
using AttendanceTracker1.Services.LogService;
using AttendanceTracker1.Services.NotificationService;
using AttendanceTracker1.Services.OvertimeService;
using AttendanceTracker1.Services.OvertimeConfigService;
using AttendanceTracker1.Services.CashAdvanceRequestService;
using System.Net;
using AttendanceTracker1.Services.OvertimeMplService;
using AttendanceTracker1.Services.FileService;
using AttendanceTracker1.Services.TeamService;
using AttendanceTracker1.Services.SupervisorService;

var builder = WebApplication.CreateBuilder(args);

//// Configure Kestrel to use HTTPS with the self-signed certificate
//builder.WebHost.ConfigureKestrel(options =>
//{
//    // Listen on IP 10.0.0.17 and port 7009
//    options.Listen(IPAddress.Parse("10.0.0.17"), 7009, listenOptions =>
//    {
//        // The path "cert.pfx" is relative to the current working directory (project root)
//        listenOptions.UseHttps("cert.pfx", "yourPfxPassword");
//    });

//    // HTTP endpoint on IP 10.0.0.17 and port 5249 (or any port you choose)
//    options.Listen(IPAddress.Parse("10.0.0.17"), 5249);
//});

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

// SERVICES
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IOvertimeService, OvertimeService>();
builder.Services.AddScoped<IOvertimeConfigService, OvertimeConfigService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICashAdvanceRequestService, CashAdvanceRequestService>();
builder.Services.AddScoped<IOvertimeMplService, OvertimeMplService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ISupervisorService, SupervisorService>();

builder.Services.AddScoped<IEmailService, EmailService>();


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
    options.AddPolicy("SupervisorOnly", policy => policy.RequireRole("Supervisor"));
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

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => e.Value.Errors.Select(err => err.ErrorMessage).ToArray()
            );

        return new OkObjectResult(AttendanceTracker1.Models.ApiResponse<object>.Success(errors, "Validation failed"));
    };

});

builder.Services.AddHttpContextAccessor();

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
