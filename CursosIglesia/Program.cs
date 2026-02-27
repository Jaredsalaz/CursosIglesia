using CursosIglesia.Components;
using CursosIglesia.Services.Interfaces;
using CursosIglesia.Services.Implementations;
using CursosIglesia.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register Services (Dependency Injection)
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
builder.Services.AddSingleton<IUserService, UserService>();

// Register ViewModels
builder.Services.AddScoped<HomeViewModel>();
builder.Services.AddScoped<CoursesViewModel>();
builder.Services.AddScoped<CourseDetailViewModel>();
builder.Services.AddScoped<ProfileViewModel>();
builder.Services.AddScoped<LearningViewModel>();
builder.Services.AddScoped<UserProfileViewModel>();

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
