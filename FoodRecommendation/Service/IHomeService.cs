using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;
using System.Linq.Expressions;

namespace FoodRecommendation.Service
{
    public interface IHomeService : IBaseService<Recipe>
    {
        Task<(List<RecipeModel> Data, int TotalItems)> GetRecipeRating(Expression<Func<Recipe, bool>> expression, int page, int pageSize);

        Task<RecipeModel> GetRecipeById(int recipeId, int userId);

        Task<bool> AddRating(Rating rating);

        Task<bool> ToggleSaveRecipe(int recipeId, int userId);

        Task<bool> SubmitReport(int userId, int recipeId, string reason);

        Task<List<NotiModel>> GetNoti(int userId);
    }
}
