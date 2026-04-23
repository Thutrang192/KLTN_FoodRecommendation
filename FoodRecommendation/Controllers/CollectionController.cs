using FoodRecommendation.Models.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;



[Authorize]
public class CollectionController : Controller
{
    private readonly FoodContext _context;

    public CollectionController(FoodContext context)
    {
        _context = context;
    }

    // ===== LIST =====
    public async Task<IActionResult> Saved(int page = 1)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        int pageSize = 8;

        var query = _context.Collections
            .Where(x => x.UserId == userId)
            .Include(x => x.Recipe)
            .ThenInclude(r => r.User)
            .OrderByDescending(x => x.CreatedAt);

        int total = await query.CountAsync();

        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SavedRecipeVM
            {
                Id = x.Recipe.RecipeId, // ✅ FIX
                Title = x.Recipe.Title,
                Author = x.Recipe.User.Username,
                Cooktime = x.Recipe.Cooktime,

                RoleUser = x.Recipe.User.RoleUser,
                ImageUrl = x.Recipe.ImageUrl
            })
            .ToListAsync();

        ViewBag.TotalItems = total;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

        return View(data);
    }

    // ===== SAVE =====
    [HttpPost]
    public async Task<IActionResult> Save(int recipeId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var exist = await _context.Collections
            .AnyAsync(x => x.UserId == userId && x.RecipeId == recipeId);

        if (!exist)
        {
            _context.Collections.Add(new Collection
            {
                UserId = userId,
                RecipeId = recipeId,
                CreatedAt = DateTime.Now // 👉 nên có
            });

            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    // ===== CHECK ĐÃ LƯU =====
    [HttpGet]
    public async Task<IActionResult> IsSaved(int recipeId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var exist = await _context.Collections
            .AnyAsync(x => x.UserId == userId && x.RecipeId == recipeId);

        return Json(exist);
    }

    // ===== BỎ LƯU =====
    [HttpPost]
    public async Task<IActionResult> Remove(int recipeId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var item = await _context.Collections
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RecipeId == recipeId);

        if (item != null)
        {
            _context.Collections.Remove(item);
            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int recipeId)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var item = await _context.Collections
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RecipeId == recipeId);

        // 👉 đã lưu → XÓA
        if (item != null)
        {
            _context.Collections.Remove(item);
            await _context.SaveChangesAsync();

            return Json(new { isSaved = false });
        }

        // 👉 chưa lưu → THÊM
        _context.Collections.Add(new Collection
        {
            UserId = userId,
            RecipeId = recipeId,
            CreatedAt = DateTime.Now
        });

        await _context.SaveChangesAsync();

        return Json(new { isSaved = true });
    }
}
