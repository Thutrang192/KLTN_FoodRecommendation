using System;
using System.Collections.Generic;

namespace FoodRecommendation.Models.Entity;

public partial class Collection
{
    public int UserId { get; set; }

    public int RecipeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}
