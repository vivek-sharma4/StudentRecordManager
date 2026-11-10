using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Serilog;
using StudentRecordManager.Data;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories;
using StudentRecordManager.Repositories.Interfaces;
using StudentRecordManager.Services;
using StudentRecordManager.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            "Logs/student-record-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            shared: true);
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = "StudentRecordManager.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// Add MVC services
builder.Services.AddControllersWithViews();

// Database connection factory
builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
// Student repository
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
// Student service
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();

// Department repository
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

// Department service
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

builder.Services.AddScoped<
    Microsoft.AspNetCore.Identity.IPasswordHasher<User>,
    Microsoft.AspNetCore.Identity.PasswordHasher<User>>();

var app = builder.Build();

app.UseSerilogRequestLogging();
// Create initial Admin account if it does not exist
if (app.Environment.IsDevelopment())
{
    var adminUsername =
        app.Configuration["AdminUser:Username"];

    var adminPassword =
        app.Configuration["AdminUser:Password"];

    if (string.IsNullOrWhiteSpace(adminUsername) ||
        string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new InvalidOperationException(
            "Admin credentials are missing from User Secrets.");
    }

    await using var scope = app.Services.CreateAsyncScope();

    var accountRepository = scope.ServiceProvider
        .GetRequiredService<IAccountRepository>();

    var passwordHasher = scope.ServiceProvider
        .GetRequiredService<IPasswordHasher<User>>();

    var existingAdmin =
        await accountRepository.GetUserByUsernameAsync(adminUsername);

    if (existingAdmin == null)
    {
        var admin = new User
        {
            Username = adminUsername
        };

        admin.PasswordHash = passwordHasher.HashPassword(
            admin,
            adminPassword);

        await accountRepository.CreateUserAsync(admin);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Student}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapGet("/database-test", async (
    IDbConnectionFactory connectionFactory) =>
{
    try
    {
        await using var connection = connectionFactory.CreateConnection();

        await connection.OpenAsync();

        var result = await connection.ExecuteScalarAsync<int>("SELECT 1");

        return Results.Ok(new{Success = true, Message = "Database connected successfully!", Result = result});
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "Database connection failed", detail: ex.Message);
    }
});

app.Run();