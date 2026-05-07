using CursosIglesia.Services.Interfaces;
using CursosIglesia.Services.Implementations;
using CursosIglesiaAPI.Services.Interfaces;
using CursosIglesiaAPI.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();

// Register Services (Dependency Injection)
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMaestroService, MaestroService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<IDailyVerseService, DailyVerseService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IForumService, ForumService>();

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Remove clock skew tolerance
    };
    
    // Add JWT Bearer event logging
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
            Console.WriteLine($"[JWT] Token received in request: {(token != null ? token.Substring(0, Math.Min(20, token.Length)) + "..." : "None")}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"[JWT] ❌ Authentication FAILED");
            Console.WriteLine($"[JWT]   Exception: {context.Exception?.GetType().Name}");
            Console.WriteLine($"[JWT]   Message: {context.Exception?.Message}");
            
            if (context.Exception is SecurityTokenExpiredException)
            {
                Console.WriteLine("[JWT]   -> Token has expired");
            }
            else if (context.Exception is SecurityTokenInvalidSignatureException)
            {
                Console.WriteLine("[JWT]   -> Invalid token signature");
            }
            else if (context.Exception is SecurityTokenInvalidIssuerException)
            {
                Console.WriteLine("[JWT]   -> Invalid issuer");
            }
            else if (context.Exception is SecurityTokenInvalidAudienceException)
            {
                Console.WriteLine("[JWT]   -> Invalid audience");
            }
            
            if (context.Exception?.InnerException != null)
            {
                Console.WriteLine($"[JWT]   Inner: {context.Exception.InnerException.Message}");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("[JWT] ✅ Token validated successfully");
            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            Console.WriteLine($"[JWT]   Subject: {userId}");
            Console.WriteLine($"[JWT]   Email: {email}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("[JWT] ⚠️  Challenge triggered");
            Console.WriteLine($"[JWT]   AuthenticateFailure: {context.AuthenticateFailure?.GetType().Name}");
            if (context.AuthenticateFailure != null)
            {
                Console.WriteLine($"[JWT]   Message: {context.AuthenticateFailure.Message}");
            }
            Console.WriteLine($"[JWT]   Error: {context.Error}");
            Console.WriteLine($"[JWT]   ErrorDescription: {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

// CORS configuration (to allow Blazor Frontend to call API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Authorization", "Content-Length", "Content-Type");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseCors("AllowBlazor");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR Hubs
app.MapHub<CursosIglesia.Hubs.ActivityHub>("/hubs/activity");
app.MapHub<CursosIglesia.Hubs.ForumHub>("/hubs/forum");

app.Run();
