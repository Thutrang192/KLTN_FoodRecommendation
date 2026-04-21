using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;
using System.Linq.Expressions;

namespace FoodRecommendation.Service
{
    public interface IAdminService : IBaseService<Recipe>
    {
        Task<List<RecipeModel>> GetAllRecipe(Expression<Func<Recipe, bool>> expression);
        Task<List<RecipeModel>> GetReportRecipe(Expression<Func<Recipe, bool>> expression);
        Task<bool> HandleReportProcessAsync(int recipeId, string actionType);
        Task<bool> RestoreAndNotifyAsync(int recipeId);
    }
    
}
