using System.ComponentModel.DataAnnotations;

namespace FoodRecommendation.Models
{
    public class UserListViewModel
    {
        // Danh sách người dùng để hiển thị trong vòng lặp
        public List<UserItemViewModel> Users { get; set; } = new List<UserItemViewModel>();

        // Các thông số phân trang
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }

        // Thông tin bộ lọc để giữ lại giá trị trên thanh tìm kiếm
        public string SearchTerm { get; set; }
        public bool IncludeDeleted { get; set; }

    }

    public class UserItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }
}
