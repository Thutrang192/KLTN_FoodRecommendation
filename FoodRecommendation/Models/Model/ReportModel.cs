using FoodRecommendation.Models.Entity;

namespace FoodRecommendation.Models.Model
{
    public class ReportModel
    {
        public int ReportId { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int RecipeId { get; set; }

        public string Reason { get; set; } = null!;

        public string? StatusReport { get; set; }

        public DateTime CreatedAt { get; set; }
        public string FormattedCreatedAt => CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";
        public DateTime? ResolvedAt { get; set; }
        public string FormattedResolvedAt => CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "";

        public virtual Recipe Recipe { get; set; } = null!;

        public virtual Account User { get; set; } = null!;
    }
}
