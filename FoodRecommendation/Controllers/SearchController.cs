using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodRecommendation.Data;
using FoodRecommendation.Models.Entity;

public class SearchController : Controller
{
    private readonly FoodContext _context;
    private readonly PythonService _pythonService;

    public SearchController(FoodContext context, PythonService pythonService)
    {
        _context = context;
        _pythonService = pythonService;
    }

    public async Task<IActionResult> Index(string query, string sort)
    {
        if (string.IsNullOrEmpty(query))
        {
            return View("Result", new List<Recipe>());
        }

        List<Recipe> recipes = new List<Recipe>();

        // ===== SEARCH BẰNG AI (NHẬP NGUYÊN LIỆU) =====
        if (query.Contains(","))
        {
            var ingredients = query
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            var recipeIds = await _pythonService.GetRecipeIds(ingredients);

            recipes = await _context.Recipes
                .Include(x => x.User)
                .Where(x => recipeIds.Contains(x.RecipeId) && x.IsDeleted == false && x.RecipeStatus == 2)
                .ToListAsync();

            // GIỮ THỨ TỰ RANKING TỪ PYTHON
            recipes = recipes
                .OrderBy(x => recipeIds.IndexOf(x.RecipeId))
                .ToList();
        }

        // ===== SEARCH THEO TÊN MÓN =====
        else
        {
            recipes = await _context.Recipes
                .Include(x => x.User)
                .Where(x => x.Title.Contains(query))
                .ToListAsync();
        }
        var favoriteDict = new Dictionary<int, int>();
        var ratingDict = new Dictionary<int, double>();

        foreach (var r in recipes)
        {
            favoriteDict[r.RecipeId] =
                _context.Collections.Count(c => c.RecipeId == r.RecipeId);

            ratingDict[r.RecipeId] =
                _context.Ratings
                    .Where(x => x.RecipeId == r.RecipeId)
                    .Average(x => (double?)x.Score) ?? 0;
        }

        ViewBag.Favorite = favoriteDict;
        ViewBag.Rating = ratingDict;


        // ===== SẮP XẾP =====

        if (sort == "favorite")
        {
            recipes = recipes
                .OrderByDescending(r =>
                    _context.Collections.Count(c => c.RecipeId == r.RecipeId))
                .ToList();
        }

        else if (sort == "rating")
        {
            recipes = recipes
                .OrderByDescending(r =>
                    _context.Ratings
                        .Where(x => x.RecipeId == r.RecipeId)
                        .Average(x => (double?)x.Score) ?? 0)
                .ToList();
        }

        ViewBag.Query = query;

        return View("Index", recipes);
    }
}