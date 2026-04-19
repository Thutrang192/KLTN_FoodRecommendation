using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FoodRecommendation.Service
{
    public class HomeService : BaseService<Recipe>, IHomeService
    {
        public HomeService(FoodContext context) : base(context) {}

        public async Task<List<RecipeModel>> GetRecipeRating(Expression<Func<Recipe, bool>> expression)
        {
            // 1. Lấy Rating và tính trung bình
            var ratings = await _context.Ratings.ToListAsync();
            var avgScoreDic = ratings.GroupBy(r => r.RecipeId)
                                     .ToDictionary(g => g.Key, g => g.Average(r => (double)r.Score));

            // 2. Lấy Recipe từ DB theo điều kiện truyền vào
            var recipes = await _context.Recipes
                                .Include(r => r.User) 
                                .Where(expression)
                                .ToListAsync();

            // 3. Map sang RecipeModel và lấy Top 8
            return recipes.Select(r => new RecipeModel
            {
                RecipeId = r.RecipeId,
                Title = r.Title,
                ImageUrl = r.ImageUrl,
                Cooktime = r.Cooktime,
                Serving = r.Serving,
                UserName = r.User?.Username,
                RoleUser = r.User?.RoleUser,
                AvgScore = (decimal)avgScoreDic.GetValueOrDefault(r.RecipeId, 0)
            })
            .OrderByDescending(r => r.AvgScore)
            .Take(8)
            .ToList();
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
                .FirstOrDefaultAsync(r => r.RecipeId == recipeId && (r.IsDeleted == null || r.IsDeleted == false));

            if (recipe == null) return null;

            var model = new RecipeModel
            {
                RecipeId = recipe.RecipeId,
                Title = recipe.Title,
                DescriptionRecipes = recipe.DescriptionRecipes,
                ImageUrl = recipe.ImageUrl,
                VideoUrl = recipe.VideoUrl,
                Cooktime = recipe.Cooktime,
                Serving = recipe.Serving,
                CreatedAt = recipe.CreatedAt,

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
                           : 0,

                TotalRatings = await _context.Ratings.CountAsync(rt => rt.RecipeId == recipeId)
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

    }
}
