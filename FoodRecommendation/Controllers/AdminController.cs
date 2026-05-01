using FoodRecommendation.Models.Entity;
using FoodRecommendation.Service;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> Recipe([FromQuery] string? search, [FromQuery] string? status, int page = 1)
        {
            int pageSize = 10;

            Expression<Func<Recipe, bool>> filter = x =>
                (string.IsNullOrEmpty(search) || x.Title.Contains(search)) &&
                (status == "active" ? (x.IsDeleted == false || x.IsDeleted == null) :
                 status == "hidden" ? (x.IsDeleted == true) : true);

            var (data, totalItems) = await _adminService.GetAllRecipe(filter, page, pageSize);

            // ViewBag cho pagination
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.PageSize = 10;

            // Giữ lại search + status khi chuyển trang
            ViewBag.Search = search;
            ViewBag.Status = status;

            ViewBag.TotalItems = totalItems;

            return View(data);
        }

        [HttpGet("Report")]
        public async Task<IActionResult> Report([FromQuery] string? search, [FromQuery] string? status, int page = 1)
        {
            int pageSize = 10;

            string currentStatus = string.IsNullOrEmpty(status) ? "pending" : status.ToLower();

            Expression<Func<Recipe, bool>> filter = r =>
                (string.IsNullOrEmpty(search) || r.Title.Contains(search)) &&

                (currentStatus == "pending"
                    ? r.Reports.Any(rep => rep.StatusReport == "Pending")

                : currentStatus == "processed"
                    ? r.Reports.Any() && r.Reports.All(rep => rep.StatusReport == "Processed")

                : r.Reports.Any());

            var (data, totalItems) = await _adminService.GetReportRecipe(filter, page, pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = Math.Max(1, (int)Math.Ceiling((double)totalItems / pageSize));
            ViewBag.TotalItems = totalItems;

            ViewBag.Search = search;
            ViewBag.Status = currentStatus;

            return View(data);
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
