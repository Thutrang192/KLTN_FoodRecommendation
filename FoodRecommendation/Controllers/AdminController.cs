using FoodRecommendation.Models.Entity;
using FoodRecommendation.Service;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;

namespace FoodRecommendation.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAdminService _adminService;
        private readonly FoodContext _db;

        public AdminController(
            ILogger<HomeController> logger,
            IAdminService adminService,
            IHttpClientFactory httpClientFactory,
            FoodContext db)
        {
            _logger = logger;
            _adminService = adminService;
            _db = db;
        }

        [HttpGet("Recipe")]
        public async Task<IActionResult> Recipe([FromQuery] string? search, [FromQuery] string? status)
        {
            Expression<Func<Recipe, bool>> filter = x =>
            (string.IsNullOrEmpty(search) || x.Title.Contains(search)) &&
            (status == "active" ? (x.IsDeleted == false || x.IsDeleted == null) :
             status == "hidden" ? (x.IsDeleted == true) : true);

            var result = await _adminService.GetAllRecipe(filter);

            return View(result);
        }

        [HttpGet("Report")]
        public async Task<IActionResult> Report(string? status)
        {
            string currentStatus = string.IsNullOrEmpty(status) ? "pending" : status.ToLower();

            Expression<Func<Recipe, bool>> filter;

            if (currentStatus == "pending")
            {
                filter = r => r.Reports.Any(rep => rep.StatusReport == "Pending");
            }
            else if (currentStatus == "processed")
            {
                filter = r => r.Reports.Any() && r.Reports.All(rep => rep.StatusReport == "Processed");
            }
            else
            {
                filter = r => r.Reports.Any();
            }

            var recipes = await _adminService.GetReportRecipe(filter);
            return View(recipes);
        }

        public class ReportProcessRequest
        {
            public int RecipeId { get; set; }
            public string ActionType { get; set; }
        }

        [HttpPost("HandleReportProcess")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> HandleReportProcess([FromBody] ReportProcessRequest request)
        {
            if (request == null || request.RecipeId == 0) return BadRequest();

            var result = await _adminService.HandleReportProcessAsync(request.RecipeId, request.ActionType);

            if (result)
            {
                return Ok(new { success = true });
            }
            return StatusCode(500, "Lỗi xử lý database");
        }

        [HttpPost("RestoreRecipe")]
        public async Task<IActionResult> RestoreRecipe([FromBody] int id)
        {
            if (id <= 0) return BadRequest();

            var result = await _adminService.RestoreAndNotifyAsync(id);

            if (result)
            {
                return Ok(new { success = true });
            }
            return StatusCode(500, "Lỗi khi khôi phục món ăn");
        }

        [HttpPost("HideRecipe")]
        public async Task<IActionResult> HideRecipe([FromBody] int id)
        {
            var result = await _adminService.HandleReportProcessAsync(id, "hide");
            if (result) return Ok(new { success = true });
            return BadRequest();
        }

    }
}
