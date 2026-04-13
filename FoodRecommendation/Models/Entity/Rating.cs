using System;
using System.Collections.Generic;

namespace FoodRecommendation.Models.Entity;

public partial class Rating
{
    public int RatingId { get; set; }

    public int UserId { get; set; }

    public int RecipeId { get; set; }

    public byte Score { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}
