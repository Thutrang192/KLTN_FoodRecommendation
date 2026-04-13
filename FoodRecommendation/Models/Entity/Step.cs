using System;
using System.Collections.Generic;

namespace FoodRecommendation.Models.Entity;

public partial class Step
{
    public int StepId { get; set; }

    public int RecipeId { get; set; }

    public int StepNumber { get; set; }

    public string StepText { get; set; } = null!;

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual ICollection<StepImage> StepImages { get; set; } = new List<StepImage>();
}
