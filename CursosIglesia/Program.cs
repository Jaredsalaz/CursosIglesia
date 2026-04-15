using CursosIglesia.Components;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.Services.Implementations.ApiClients;
using CursosIglesia.ViewModels;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5114";

// Authorization & LocalStorage
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "BlazorAuth";
    options.DefaultChallengeScheme = "BlazorAuth";
}).AddCookie("BlazorAuth", options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
});
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddSingleton<CursosIglesia.Services.InMemoryTokenStore>();
builder.Services.AddTransient<JwtAuthorizationHandler>();

// Register API Clients (HttpClient with Token Handler)
builder.Services.AddScoped<IAuthService, ApiAuthService>();
builder.Services.AddHttpClient<IAuthService, ApiAuthService>(client => client.BaseAddress = new Uri(apiBaseUrl));

// For other services, use the JwtAuthorizationHandler to automatically add tokens
builder.Services.AddScoped<ICourseService, ApiCourseService>();
builder.Services.AddHttpClient<ICourseService, ApiCourseService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddScoped<ICategoryService, ApiCategoryService>();
builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddScoped<ITestimonialService, ApiTestimonialService>();
builder.Services.AddHttpClient<ITestimonialService, ApiTestimonialService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddScoped<IEnrollmentService, ApiEnrollmentService>();
builder.Services.AddHttpClient<IEnrollmentService, ApiEnrollmentService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddScoped<IUserService, ApiUserService>();
builder.Services.AddHttpClient<IUserService, ApiUserService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddScoped<IMaestroService, ApiMaestroService>();
builder.Services.AddHttpClient<IMaestroService, ApiMaestroService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddScoped<IAdminService, ApiAdminService>();
builder.Services.AddHttpClient<IAdminService, ApiAdminService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddScoped<ApiQuizService>();
builder.Services.AddHttpClient<ApiQuizService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<IDailyVerseService, ApiDailyVerseService>(client => client.BaseAddress = new Uri(apiBaseUrl));

// Register ViewModels
builder.Services.AddScoped<HomeViewModel>();
builder.Services.AddScoped<CoursesViewModel>();
builder.Services.AddScoped<CourseDetailViewModel>();
builder.Services.AddScoped<ProfileViewModel>();
builder.Services.AddScoped<LearningViewModel>();
builder.Services.AddScoped<UserProfileViewModel>();
builder.Services.AddScoped<LoginViewModel>();
builder.Services.AddScoped<RegisterViewModel>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
