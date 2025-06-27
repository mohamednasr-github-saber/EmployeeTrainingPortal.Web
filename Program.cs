using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using EmployeeTrainingPortal.Models;
using EmployeeTrainingPortal.DAL.Repositories.IRepositories;
using EmployeeTrainingPortal.DAL.Repositories;
using EmployeeTrainingPortal.Utility;
using ECommerce512.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 3. Register Services (Repositories)
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
builder.Services.AddScoped<IApplicationUserOtpRepository, ApplicationUserOtpRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
// 4. Email Sender
builder.Services.AddTransient<IEmailSender, EmailSender>();

// 5. MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 6. Middleware
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 7. Routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Account}/{action=Login}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
