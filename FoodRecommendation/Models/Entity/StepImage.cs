using System;
using System.Collections.Generic;

namespace FoodRecommendation.Models.Entity;

public partial class StepImage
{
    public int StepImageId { get; set; }

    public int StepId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public virtual Step Step { get; set; } = null!;
}
