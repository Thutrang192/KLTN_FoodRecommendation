using FoodRecommendation.Constant;
using FoodRecommendation.Models.Model;
using FoodRecommendation.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using FoodRecommendation.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FoodRecommendation.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IBaseService<Account> _userService;
        private readonly FoodContext _context;

        public AccountController(
            IAuthService authService,
            IBaseService<Account> userService,
            FoodContext context)
        {
            _authService = authService;
            _userService = userService;
            _context = context; 
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.ToastType = Constants.None;

            if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
            {
                ViewBag.ToastMessage = TempData["ToastMessage"];
                ViewBag.ToastType = TempData["ToastType"];

                TempData.Remove("ToastMessage");
                TempData.Remove("ToastType");
            }

            return View(new AccountModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ViewData["ReturnUrl"] = returnUrl;

            var user = await _authService.AuthenticationUser(model);

            if (user == null)
            {
                ModelState.AddModelError("Email", " ");
                ModelState.AddModelError("Password", "Email hoặc mật khẩu không chính xác!");
                ViewBag.ToastType = Constants.Error;

                return View(model);
            }

            if (user.IsActivated == false)
            {
                ModelState.AddModelError("Email", " ");
                ModelState.AddModelError("Password", "Tài khoản của bạn đang bị khóa. Vui lòng liên hệ admin để biết thêm chi tiết.");

                return View(model);
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleUser ?? "User"),
                new Claim("Avatar", user.AvatarUrl ?? "")
            }; ;

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }


        // Get: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userService.Get(a => a.Username == model.Username || a.Email == model.Email);

                if (existingUser != null)
                {
                    if (existingUser.Username == model.Username)
                        ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");

                    if (existingUser.Email == model.Email)
                        ModelState.AddModelError("Email", "Email đã tồn tại");

                    return View(model);
                }
                
                await _authService.InsertUser(model);

                TempData["ToastMessage"] = "Đăng ký tài khoản thành công.";
                TempData["ToastType"] = Constants.Success;

                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePass()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> ChangePass(PasswordModel model)
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Accounts.FindAsync(userId);
            if (user == null) return NotFound();

            var model = new UpdateProfileVM
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl
            };

            ViewBag.Username = user.Username;

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(UpdateProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Username = User.Identity?.Name;
                return View(model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Accounts.FindAsync(userId);
            if (user == null) return NotFound();

            // ===== UPDATE FULLNAME =====
            user.FullName = model.FullName?.Trim();

            // ===== VALIDATE USERNAME =====
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập không được để trống");
            }

            var newUsername = model.Username?.Trim();

            var existUsername = await _context.Accounts
                .AnyAsync(x => x.Username == newUsername && x.UserId != user.UserId);

            if (existUsername)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
            }

            // ===== VALIDATE EMAIL =====
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email không được để trống");
            }

            var newEmail = model.Email?.Trim();

            var existEmail = await _context.Accounts
                .AnyAsync(x => x.Email == newEmail && x.UserId != user.UserId);

            if (existEmail)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
            }

            // ===== Nếu có lỗi thì return =====
            if (!ModelState.IsValid)
            {
                ViewBag.Username = user.Username;
                return View(model);
            }

            // ===== UPDATE USERNAME + EMAIL =====
            user.Username = newUsername;
            user.Email = newEmail;

            // ===== REMOVE AVATAR =====
            if (model.RemoveAvatar)
            {
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }

                user.AvatarUrl = null;
            }
            // ===== UPLOAD AVATAR =====
            else if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatars");
                Directory.CreateDirectory(folder);

                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.AvatarUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var fileName = Guid.NewGuid() + Path.GetExtension(model.AvatarFile.FileName);
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                user.AvatarUrl = "/images/avatars/" + fileName;
            }

            // ===== SAVE DB =====
            await _context.SaveChangesAsync();

            // ===== UPDATE HEADER =====
            await RefreshSignIn(user);

            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công";

            return RedirectToAction("EditProfile");
        }
        private async Task RefreshSignIn(Account user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleUser ?? "User"),
                new Claim("Avatar", user.AvatarUrl ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordVM());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ✅ Lấy userId an toàn
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdStr);

            var user = await _context.Accounts.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
            {
                return NotFound();
            }

            // ❗ CHECK mật khẩu hiện tại
            bool isCorrectPassword = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash);

            if (!isCorrectPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                return View(model);
            }

            // ❗ KHÔNG cho trùng mật khẩu cũ
            bool isSamePassword = BCrypt.Net.BCrypt.Verify(model.NewPassword, user.PasswordHash);

            if (isSamePassword)
            {
                ModelState.AddModelError("NewPassword", "Mật khẩu mới không được trùng mật khẩu cũ");
                return View(model);
            }

            // ❗ UPDATE mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            _context.Accounts.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";

            return RedirectToAction(nameof(ChangePassword));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserList(int page = 1)
        {
            int pageSize = 10;

            var query = _context.Accounts.AsQueryable();

            int totalItems = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalItems = totalItems;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(data);
        }

    }
}
