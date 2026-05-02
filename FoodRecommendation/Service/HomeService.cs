using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FoodRecommendation.Service
{
    public class HomeService : BaseService<Recipe>, IHomeService
    {
        public HomeService(FoodContext context) : base(context) {}

        public async Task<(List<RecipeModel> Data, int TotalItems)> GetRecipeRating(Expression<Func<Recipe, bool>> expression, int page, int pageSize)
        {
            // 1. Lấy Top 200 món thỏa mãn điều kiện và sắp xếp theo điểm trung bình
            var top200Recipes = await _context.Recipes
                .Include(r => r.User)
                .Where(expression)
                .Select(r => new RecipeModel
                {
                    RecipeId = r.RecipeId,
                    Title = r.Title,
                    ImageUrl = r.ImageUrl,
                    Cooktime = r.Cooktime,
                    Serving = r.Serving,
                    UserName = r.User.Username,
                    RoleUser = r.User.RoleUser,
                    // Tính trung bình cộng trực tiếp trên Database
                    AvgScore = (decimal)(_context.Ratings
                        .Where(rat => rat.RecipeId == r.RecipeId)
                        .Average(rat => (double?)rat.Score) ?? 0)
                })
                .OrderByDescending(r => r.AvgScore)
                .Take(200) // Giới hạn lấy 200 món tốt nhất
                .ToListAsync();

            // 2. Tính toán tổng số item trong tập 200 món này
            int totalItems = top200Recipes.Count;

            // 3. Thực hiện phân trang (In-memory paging trên tập 200 món)
            var pagedData = top200Recipes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedData, totalItems);
        }

        public async Task<RecipeModel?> GetRecipeById(int recipeId, int UserId)
        {
            var recipe = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.Ingredients)
                .Include (r => r.Steps.OrderBy(s => s.StepNumber))
                    .ThenInclude(s => s.StepImages)
                .Include(r => r.Ratings)                    
                    .ThenInclude(rt => rt.User)
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId);

            if (recipe == null) return null;

            var totalRatings = await _context.Ratings
                .CountAsync(rt => rt.RecipeId == recipeId);

            var isReported = UserId > 0
                && await _context.Reports
                    .AnyAsync(rp => rp.RecipeId == recipeId && rp.UserId == UserId);
            
            var model = new RecipeModel
            {
                RecipeId = recipe.RecipeId,
                Title = recipe.Title,
                DescriptionRecipes = recipe.DescriptionRecipes,
                ImageUrl = recipe.ImageUrl,
                VideoUrl = recipe.VideoUrl,
                Cooktime = recipe.Cooktime,
                Serving = recipe.Serving,
                CreatedAt = recipe.CreatedAt ?? DateTime.Now,
                RoleUser = recipe.User.RoleUser,

                UserName = recipe.User?.Username ?? "Ẩn danh",

                Ingredients = recipe.Ingredients.Select(i => new IngredientModel
                {
                    Quantity = i.Quantity,
                    IngredientsText = i.IngredientsText
                }).ToList(),

                Steps = recipe.Steps.Select(s => new StepModel
                {
                    StepNumber = s.StepNumber,
                    StepText = s.StepText,
                    ImgUrl = s.StepImages.Select(img => img.ImageUrl).ToList()
                }).ToList(),

                // Chỉ có 5 rating mới nhất
                Ratings = recipe.Ratings
                    .OrderByDescending(rt => rt.CreatedAt)
                    .Take(5)
                    .Select(rt => new RatingModel
                    {
                        RatingId = rt.RatingId,
                        UserId = rt.UserId,
                        UserName = rt.User?.Username ?? "Người dùng",
                        AvatarUrl = rt.User?.AvatarUrl ?? "",
                        Score = rt.Score,
                        Comment = rt.Comment,
                        CreatedAt = rt.CreatedAt
                    }).ToList(),

                // Tính điểm trung bình
                AvgScore = recipe.Ratings.Any()
                           ? (decimal)recipe.Ratings.Average(rt => rt.Score)
                           : 0m,

                TotalRatings = totalRatings,
                IsReported = isReported
            }; 

            // Lấy rating của người dùng hiện tại 
            model.YourRating = await _context.Ratings
                .Where(rt => rt.RecipeId == recipeId && rt.UserId == UserId)
                .Select(rt => new RatingModel
                {
                    RatingId = rt.RatingId,
                    UserId = rt.UserId,
                    Score = rt.Score,
                    Comment = rt.Comment,
                    CreatedAt = rt.CreatedAt
                })
                .FirstOrDefaultAsync();

            return model;
        }

        public async Task<bool> AddRating(Rating rating)
        {
            // Kiểm tra xem đã đánh giá chưa
            var isExisted = await _context.Ratings.AnyAsync(r =>
                r.UserId == rating.UserId && r.RecipeId == rating.RecipeId);

            if (isExisted) return false;

            await _context.Ratings.AddAsync(rating);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleSaveRecipe(int recipeId, int userId)
        {
            var existingSave = await _context.Collections
                .FirstOrDefaultAsync(s => s.RecipeId == recipeId && s.UserId == userId);

            if (existingSave != null)
            {
                // 2. Nếu đã tồn tại -> Xóa (Unsave)
                _context.Collections.Remove(existingSave);
                await _context.SaveChangesAsync();
                return false; 
            }
            else
            {
                var newSave = new Collection
                {
                    RecipeId = recipeId,
                    UserId = userId,
                    CreatedAt = DateTime.Now 
                };

                await _context.Collections.AddAsync(newSave);
                await _context.SaveChangesAsync();
                return true; 
            }
        }

        public async Task<bool> SubmitReport(int userId, int recipeId, string reason)
        {
            try
            {
                var existingReport = await _context.Reports
                    .AnyAsync(r => r.UserId == userId && r.RecipeId == recipeId);

                if (existingReport)
                {
                    return false; 
                }

                // kiểm tra trạng thái Verified
                var recipe = await _context.Recipes.FindAsync(recipeId);
                if (recipe == null) return false;

                var report = new Report
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    Reason = reason,
                    StatusReport = "Pending",
                    CreatedAt = DateTime.Now
                };

                if (recipe.IsVerified == true)
                {
                    report.StatusReport = "Processed"; 
                    report.ResolvedAt = DateTime.Now;

                    var noti = new Notification
                    {
                        UserId = userId,
                        RecipeId = recipeId,
                        Content = $"Cảm ơn bạn, món ăn '{recipe.Title}' đã được đội ngũ quản trị kiểm duyệt và xác nhận an toàn trước đó.",
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(noti);
                }

                _context.Reports.Add(report);

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<NotiModel>> GetNoti(int userId)
        {
            var result =  await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotiModel
                {
                    RecipeId = n.RecipeId,
                    Title = n.Recipe.Title,
                    ImageUrl = n.Recipe.ImageUrl,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt ?? DateTime.Now,
                })
                .ToListAsync();

            return result ?? new List<NotiModel>();
        }

    }
}
