using System.Diagnostics;
using FoodRecommendation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FoodRecommendation.Service;
using System.Security.Claims;

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
