using FoodRecommendation.Models.Entity;

namespace FoodRecommendation.Models.Model
{
    public class NotiModel
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }

        public string Content { get; set; } = null!;
        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
        public string FormattedCreatedAt => CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";

    }
}
