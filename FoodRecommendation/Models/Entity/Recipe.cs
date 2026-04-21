using System;
using System.Collections.Generic;

namespace FoodRecommendation.Models.Entity;

public partial class Recipe
{
    public int RecipeId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string? DescriptionRecipes { get; set; }

    public string? ImageUrl { get; set; }

    public string? VideoUrl { get; set; }

    public int? Cooktime { get; set; }

    public int? Serving { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int RecipeStatus { get; set; }

    public bool? IsDeleted { get; set; }

    public bool IsVerified { get; set; }

    public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();

    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public virtual ICollection<Step> Steps { get; set; } = new List<Step>();

    public virtual Account User { get; set; } = null!;
}
