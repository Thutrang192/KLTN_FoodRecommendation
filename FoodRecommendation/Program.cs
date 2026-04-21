using FoodRecommendation.Models;
using FoodRecommendation.Models.Entity;
using FoodRecommendation.SeedData;
using FoodRecommendation.Service;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ==================== CONNECTION STRING ====================
var connectionString = builder.Configuration.GetConnectionString("sql");

// ==================== REGISTER SERVICES ====================
builder.Services.AddDbContext<FoodContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<DbSeeder>();                    
builder.Services.AddScoped(typeof(IBaseService<>), typeof(BaseService<>)); 
builder.Services.AddSession();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "FoodRecCookie"; // Đặt tên để tránh trùng lặp
}); builder.Services.AddAuthorization();

builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ==================== SEED DATA (Chạy một lần khi khởi động) ====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var seeder = services.GetRequiredService<DbSeeder>();
        await seeder.SeedAsync();        // Gọi hàm seed
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Loi khi seed");
        Console.WriteLine(ex.ToString());
    }
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
