using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;

namespace FoodRecommendation.Service
{
    public interface IAuthService
    {
        Task InsertUser(RegisterModel model);
        Task<Account> AuthenticationUser(AccountModel model);
        Task<string> HashPassword(string value);
        Task<bool> ValidateHashPassword(string value, string hash);
    }
}