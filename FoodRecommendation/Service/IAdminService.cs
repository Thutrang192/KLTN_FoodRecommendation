using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;
using System.Linq.Expressions;

namespace FoodRecommendation.Service
{
    public interface IAdminService : IBaseService<Recipe>
    {
        Task<(List<RecipeModel> Data, int TotalItems)> GetAllRecipe(Expression<Func<Recipe, bool>> expression, int page, int pageSize);
        Task<(List<RecipeModel> Data, int TotalItems)> GetReportRecipe(Expression<Func<Recipe, bool>> expression, int page, int pageSize);
        Task<bool> HandleReportProcessAsync(int recipeId, string actionType);
        Task<bool> RestoreAndNotifyAsync(int recipeId);
    }
    
}
