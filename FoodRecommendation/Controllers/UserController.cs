using FoodRecommendation.Constant;
using FoodRecommendation.Models;
using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;
using FoodRecommendation.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using FoodRecommendation.Models.Model;

namespace FoodRecommendation.Controllers
{


    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly FoodContext _context;

        public UserController(FoodContext context)
        {
            _context = context;
        }

        // ================= EDIT GET =================
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Accounts.FindAsync(id);
            if (user == null) return NotFound();

            var model = new UpdateUserVM
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                IsActivated = user.IsActivated ?? true
            };

            return View(model);
        }

        // ================= EDIT POST =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Accounts.FindAsync(model.UserId);
            if (user == null) return NotFound();

            // ===== CHECK USERNAME =====
            var usernameExist = await _context.Accounts
                .AnyAsync(x => x.Username == model.Username && x.UserId != model.UserId);

            if (usernameExist)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
            }

            // ===== CHECK EMAIL =====
            var emailExist = await _context.Accounts
                .AnyAsync(x => x.Email == model.Email && x.UserId != model.UserId);

            if (emailExist)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
            }

            if (!ModelState.IsValid)
                return View(model);

            // ===== UPDATE =====
            user.Username = model.Username.Trim();
            user.FullName = model.FullName?.Trim();
            user.Email = model.Email.Trim();
            user.IsActivated = model.IsActivated;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
            return RedirectToAction("UserList", "User");
        }
        public async Task<IActionResult> UserList()
        {
            var users = await _context.Accounts.ToListAsync();

            ViewBag.TotalItems = users.Count;

            return View("~/Views/Account/UserList.cshtml", users);
        }
    }
}