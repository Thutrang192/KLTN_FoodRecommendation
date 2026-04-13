using System;
using System.Collections.Generic;

namespace FoodRecommendation.Models.Entity;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public int RecipeId { get; set; }

    public string ContentReport { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}
