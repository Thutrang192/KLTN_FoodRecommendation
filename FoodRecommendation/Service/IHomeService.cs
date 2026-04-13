using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;
using System.Linq.Expressions;

namespace FoodRecommendation.Service
{
    public interface IHomeService : IBaseService<Recipe>
    {
        Task<List<RecipeModel>> GetRecipeRating(Expression<Func<Recipe, bool>> expression);
        //Task<List<RecipeModel>> GetRecipeAll(Expression<Func<Recipe, bool>> expression);
        Task<RecipeModel> GetRecipeById(int recipeId, int userId);
    }
}
