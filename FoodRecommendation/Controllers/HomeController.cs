using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FoodRecommendation.Service;
using System.Security.Claims;
using FoodRecommendation.Models.Entity;
using FoodRecommendation.Constant;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using FoodRecommendation.Models.Model;

namespace FoodRecommendation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;
        private readonly HttpClient _httpClient;
        private readonly FoodContext _db;
        private readonly PythonService _pythonService;

        public HomeController(
            ILogger<HomeController> logger, 
            IHomeService homeService,
            IHttpClientFactory httpClientFactory,
            FoodContext db,
            PythonService pythonService)
        {
            _logger = logger;
            _homeService = homeService;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:8000/");
            _db = db;
            _pythonService = pythonService;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Index() 
        {
            var top8Recipes = await _homeService.GetRecipeRating(x =>
                x.RecipeStatus == 2 && x.IsDeleted != true);
            return View(top8Recipes);
        }
        public async Task<IActionResult> Detail(int id)
        {
            int currentUserId = 0;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    int.TryParse(userIdClaim, out currentUserId);
                }
            }
            var model = await _homeService.GetRecipeById(id, currentUserId); 
            
            if (model == null)
                return NotFound();
            
            if (currentUserId != 0)
            {
                model.IsSaved = await _db.Collections
                    .AnyAsync(x => x.UserId == currentUserId && x.RecipeId == id);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ReportRecipe(int recipeId, string reason)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, redirectUrl = Url.Action("Login", "Account") });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _homeService.SubmitReport(userId, recipeId, reason);

            Console.WriteLine($"UserId: {userId}, RecipeId: {recipeId}");

            if (result)
            {
                return Json(new { success = true, message = "Báo cáo thành công!" });
            }

            return Json(new { success = false, message = "Bạn đã báo cáo món này rồi." });
        }

        [Authorize]
        public IActionResult CreateRecipe()
        {
            return View();
        }

        [HttpPost]
        [Authorize] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int RecipeId, int RatingValue, string Comment)
        {
            if (RatingValue <= 0 || string.IsNullOrWhiteSpace(Comment))
            {
                TempData["ToastMessage"] = "Dữ liệu không hợp lệ!";
                TempData["ToastType"] = Constants.Error;
                return RedirectToAction("Detail", new { id = RecipeId });
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var newRating = new Rating
            {
                RecipeId = RecipeId,
                UserId = userId,
                Score = (byte)RatingValue,
                Comment = Comment, 
                CreatedAt = DateTime.Now
            };

            // 3. Lưu vào Database
            var result = await _homeService.AddRating(newRating);

            if (result)
            {
                TempData["ToastMessage"] = "Đăng bình luận thành công!";
                TempData["ToastType"] = Constants.Success;

                return RedirectToAction("Detail", new { id = RecipeId });
            } else
            {
                TempData["ToastMessage"] = "Bạn đã đánh giá món ăn này rồi!";
                TempData["ToastType"] = Constants.Warning;
            }

            return RedirectToAction("Detail", "Home", new { id = RecipeId });

        }

        public IActionResult Privacy()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetRelatedRecipe(int id)
        {
            try
            {
                var ingredients = await _db.Ingredients
                    .Where(x => x.RecipeId == id)
                    .Select(x => x.IngredientsText)
                    .ToListAsync();

                if (ingredients == null || ingredients.Count == 0)
                    return Content("<p>Không có món ăn gợi ý phù hợp.</p>");

                var recipeIds = await _pythonService.GetRecipeIds(ingredients);

                if (recipeIds == null || recipeIds.Count == 0)
                    return Content("<p>Không có món ăn gợi ý phù hợp.</p>");

                // bỏ món đang xem
                recipeIds = recipeIds
                    .Where(x => x != id)
                    .Take(6)
                    .ToList();

               var recipesFromDb = await _db.Recipes
                .Include(r => r.User)
                .Where(r => recipeIds.Contains(r.RecipeId)
                            && r.IsDeleted == false
                            && r.RecipeStatus == 2)
                .ToListAsync();

                var sortedRelatedRecipes = recipeIds
                    .Select(rid => recipesFromDb.FirstOrDefault(r => r.RecipeId == rid))
                    .Where(r => r != null)
                    .Select(r => new RecipeModel
                    {
                        RecipeId = r.RecipeId,
                        Title = r.Title,
                        ImageUrl = r.ImageUrl,
                        Cooktime = r.Cooktime,
                        Serving = r.Serving,
                        UserName = r.User?.Username,
                        RoleUser = r.User?.RoleUser
                    })
                    .ToList();

                return PartialView("_RelatedRecipePartial", sortedRelatedRecipes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GetRelatedRecipe: " + ex.Message);
                return Content("<p>Lỗi hệ thống khi gợi ý món ăn.</p>");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = int.Parse(userIdClaim);

            var notifications = await _homeService.GetNoti(userId);
            
            if (notifications == null)
            {
                notifications = new List<NotiModel>();
            }

            return View(notifications);
        }

    }
}
