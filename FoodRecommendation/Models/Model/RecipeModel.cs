using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodRecommendation.Models.Model
{
    public class RecipeModel
    {
        // thong tin mon an
        public int RecipeId { get; set; }
        public string Title { get; set; } = null!;
        public string? DescriptionRecipes { get; set; }
        public string? ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public int? Cooktime { get; set; }
        public int? Serving { get; set; }
        public DateTime? CreatedAt { get; set; }

        [NotMapped]
        public decimal AvgScore { get; set; }

        // thong tin nguoi tao
        public string UserName { get; set; }
        public string RoleUser { get; set; }
        public string? AvatarUrl { get; set; }

        // danh sach nguyen lieu
        public List<IngredientModel> Ingredients { get; set; } = new();

        // danh sach cac buoc
        public List<StepModel> Steps { get; set; } = new();

        // danh gia
        public List<RatingModel> Ratings { get; set; } = new();
        public RatingModel? YourRating { get; set; }
        public int TotalRatings { get; set; }
    }
}
