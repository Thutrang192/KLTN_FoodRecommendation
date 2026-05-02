using System.ComponentModel.DataAnnotations;

namespace FoodRecommendation.Models.Model
{

    public class UpdateUserVM
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; }

        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public bool IsActivated { get; set; }
    }
}
