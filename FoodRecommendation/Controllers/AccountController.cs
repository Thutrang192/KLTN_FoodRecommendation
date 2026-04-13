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

namespace FoodRecommendation.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IBaseService<Account> _userService;

        public AccountController(
            IAuthService authService,
            IBaseService<Account> userService)
        {
            _authService = authService;
            _userService = userService;
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
                ViewBag.ToastMessage = "Tài khoản đã bị khóa!";
                ViewBag.ToastType = Constants.Error;

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

        //[HttpGet]
        //[Authorize]
        //public async Task<IActionResult> Infomation()
        //{
        //    ViewBag.ToastType = Constants.None;
        //    if (TempData["ToastMessage"] != null && TempData["ToastType"] != null)
        //    {
        //        ViewBag.ToastMessage = TempData["ToastMessage"];
        //        ViewBag.ToastType = TempData["ToastType"];

        //        TempData.Remove("ToastMessage");
        //        TempData.Remove("ToastType");
        //    }

        //}

        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> Infomation(UserInfomationModel model)
        //{

        //}
    }
}
