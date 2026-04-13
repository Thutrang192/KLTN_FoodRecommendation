using System.ComponentModel.DataAnnotations;

namespace FoodRecommendation.Models.Model
{
    public class RatingModel
    {
        public int RatingId { get; set; }
        public int RecipeId { get; set; }
        public int UserId { get; set; }
        public string AvatarUrl { get; set; }
        public string UserName { get; set; } = string.Empty;
        
        [Range(1, 5)]
        public byte Score { get; set; }           
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FormattedCreatedAt => CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";
    }
}
