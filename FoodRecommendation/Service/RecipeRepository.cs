using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodRecommendation.Models.Entity;
using Microsoft.EntityFrameworkCore;

public class RecipeRepository
{
    private readonly FoodContext _context;

    public RecipeRepository(FoodContext context)
    {
        _context = context;
    }

    public async Task<List<Recipe>> GetByIds(List<int> ids)
{
    var recipes = await _context.Recipes
        .Include(x => x.User)
        .Where(x => ids.Contains(x.RecipeId))
        .ToListAsync();

    return recipes
        .OrderBy(x => ids.IndexOf(x.RecipeId))
        .ToList();
}
}