using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace FoodRecommendation.Service
{
    public class AdminService : BaseService<Recipe>, IAdminService
    {
        public AdminService(FoodContext context) : base(context) { }

        public async Task<(List<RecipeModel> Data, int TotalItems)> GetAllRecipe(
            Expression<Func<Recipe, bool>> expression, 
            int page, 
            int pageSize)
        {
            var query = _context.Recipes.Where(expression);

            int totalItems = await query.CountAsync();

            var data = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page-1)*pageSize)
                .Take(pageSize)
                .Select(r => new RecipeModel
                {
                    RecipeId = r.RecipeId,
                    Title = r.Title,
                    ImageUrl = r.ImageUrl,
                    CreatedAt = r.CreatedAt ?? DateTime.Now,
                    IsDeleted = r.IsDeleted ?? false,

                    Ingredients = r.Ingredients.Select(i => new IngredientModel
                    {
                        Quantity = i.Quantity,
                        IngredientsText = i.IngredientsText
                    }).ToList(),
                })
                .ToListAsync();

            return (data, totalItems);
        }
        
        public async Task<(List<RecipeModel> Data, int TotalItems)> GetReportRecipe(
            Expression<Func<Recipe, bool>> expression,
            int page,
            int pageSize)
        {
            var query = _context.Recipes.AsNoTracking().Where(expression);

            int totalItems = await query.CountAsync();

            var data = await query
                .OrderByDescending(r => r.Reports.Count())
                .ThenByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RecipeModel
                {
                    RecipeId = r.RecipeId,
                    Title = r.Title,
                    ImageUrl = r.ImageUrl,
                    CreatedAt = r.CreatedAt ?? DateTime.Now,

                    TotalReport = r.Reports.Count(),
                    IsReported = r.Reports.Any(),

                    // map danh sach bao cao
                    Reports = r.Reports
                        .OrderByDescending(rep => rep.CreatedAt)
                        .Select(rep => new ReportModel
                    {
                        Username = rep.User.Username,
                        Email = rep.User.Email,
                        Reason = rep.Reason,
                        StatusReport = rep.StatusReport,
                        CreatedAt = rep.CreatedAt ?? DateTime.Now,
                    }).ToList(),
                })
                .ToListAsync();

            return (data, totalItems);
        }

        public async Task<bool> HandleReportProcessAsync(int recipeId, string actionType)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Reports)
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId);

            if (recipe == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                string mess = actionType == "hide"
                ? $"Cảm ơn bạn đã phản hồi! Món '{recipe.Title}' tạm thời đã được ẩn để đảm bảo tiêu chuẩn cộng đồng."
                : $"Cảm ơn bạn đã để tâm đến món '{recipe.Title}'. Sau khi xem xét kỹ lưỡng và nhận thấy nội dung vẫn phù hợp với quy định. Hy vọng bạn tiếp tục đồng hành cùng Chef!"; 

                if (recipe.Reports != null && recipe.Reports.Any())
                {
                    foreach (var report in recipe.Reports)
                    {
                        report.StatusReport = "Processed";
                        report.ResolvedAt = DateTime.Now;

                        var noti = new Notification
                        {
                            UserId = report.UserId,
                            RecipeId = recipe.RecipeId,
                            Content = mess,
                            CreatedAt = DateTime.Now
                        };

                        _context.Notifications.Add(noti);
                    }
                }

                if (actionType == "hide")
                {
                    recipe.IsDeleted = true;
                    recipe.IsVerified = false;
                }
                else if (actionType == "safe")
                {
                    recipe.IsVerified = true;  
                    recipe.IsDeleted = false; 
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Console.WriteLine("CHI TIET LOI DB: " + innerError);
                return false;
            }
        }

        public async Task<bool> RestoreAndNotifyAsync(int recipeId)
        {
            var recipe = await _context.Recipes
                .Include(r => r.Reports)
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId);

            if (recipe == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                recipe.IsDeleted = false;
                recipe.IsVerified = true;

                if (recipe.Reports != null && recipe.Reports.Any())
                {
                    string notifyContent = $"Chúng tôi đã kiểm tra lại báo cáo của bạn về món '{recipe.Title}'. Sau khi xem xét thêm, món ăn đã được mở lại vì phù hợp với tiêu chuẩn cộng đồng.";

                    foreach (var report in recipe.Reports)
                    {
                        var noti = new Notification
                        {
                            UserId = report.UserId,
                            RecipeId = recipe.RecipeId,
                            Content = notifyContent,
                            CreatedAt = DateTime.Now
                        };
                        _context.Notifications.Add(noti);

                        report.StatusReport = "Processed";
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("Lỗi khi khôi phục món: " + ex.Message);
                return false;
            }
        }
    }
}
