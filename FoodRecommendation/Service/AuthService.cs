using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;
using AutoMapper;
using BCrypt.Net;

namespace FoodRecommendation.Service
{

    public class AuthService : IAuthService
    {

        private readonly IBaseService<Account> _userService;

        public AuthService(IBaseService<Account> userService)
        {
            _userService = userService;
        }

        public async Task InsertUser(RegisterModel model)
        {
            var acc = new Account
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = model.Password,
            };

            if (model.RoleEnumSeeder == null)
            {
                acc.RoleUser = Constant.RoleEnum.User.ToString();
            }
            else
            {
                acc.RoleUser = model.RoleEnumSeeder.ToString();
            }
            acc.PasswordHash = await HashPassword(model.Password);
            acc.IsActivated = true;

            await _userService.Insert(acc);
        }

        public async Task<Account> AuthenticationUser(AccountModel model)
        {
            if (string.IsNullOrEmpty(model.Email)) return null;

            var keyword = model.Email.ToLower().Trim();

            var user = await _userService.GetList(x =>
                (x.Username != null && x.Username.ToLower().Trim() == keyword) ||
                (x.Email != null && x.Email.ToLower().Trim() == keyword)
            );

            if (user != null && user.Any())
            {
                foreach (var u in user)
                {
                    if (await ValidateHashPassword(model.Password, u.PasswordHash))
                    {
                        return u;
                    }
                }
            }

            return null;
        }

        /// Decode password
        public Task<string> HashPassword(string value)
        {
            return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(value, BCrypt.Net.BCrypt.GenerateSalt(12)));
        }

        /// Verify password
        public Task<bool> ValidateHashPassword(string value, string hash)
        {
            return Task.FromResult(BCrypt.Net.BCrypt.Verify(value, hash));
        }

    }
}
