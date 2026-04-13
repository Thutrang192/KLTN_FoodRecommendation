using System;
using System.Collections.Generic;

namespace FoodRecommendation.Models.Entity;

public partial class Report
{
    public int ReportId { get; set; }

    public int UserId { get; set; }

    public int RecipeId { get; set; }

    public string Reason { get; set; } = null!;

    public string? StatusReport { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual Account User { get; set; } = null!;
}
