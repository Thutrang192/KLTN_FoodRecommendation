using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FoodRecommendation.Service;
using System.Security.Claims;
using FoodRecommendation.Models.Entity;
using FoodRecommendation.Constant;

namespace FoodRecommendation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;

        public HomeController(ILogger<HomeController> logger, IHomeService homeService)
        {
            _logger = logger;
            _homeService = homeService;
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

            return View(model);
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

        public IActionResult Noti()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


       
    }
}
