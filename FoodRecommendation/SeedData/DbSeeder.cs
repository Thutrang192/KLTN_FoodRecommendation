using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;
using FoodRecommendation.Service;
using Microsoft.EntityFrameworkCore;
using FoodRecommendation.Constant;

namespace FoodRecommendation.SeedData
{
    public class DbSeeder
    {
        private readonly IAuthService _authService;
        private readonly FoodContext _context;

        public DbSeeder(IAuthService authService, FoodContext contect)
        {
            _authService = authService;
            _context = contect;
        }

        public async Task SeedAsync()                 
        {
            try
            {
                // Kiểm tra đã có tài khoản nào chưa
                if (await _context.Accounts.AnyAsync())
                {
                    return;   // Đã có dữ liệu rồi thì không seed nữa
                }

                var admin = new RegisterModel
                {
                    Username = "admin",
                    FullName = "Administrator",
                    Email = "admin@example.com",
                    RoleEnumSeeder = RoleEnum.Admin,     
                    Password = "Admin123@"
                };

                await _authService.InsertUser(admin);

                Console.WriteLine("Đã seed tài khoản admin thành công!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seed data lỗi: {ex.Message}");
            }
        }
    }
}
