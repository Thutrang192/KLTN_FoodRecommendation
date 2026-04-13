using System;
using System.Collections.Generic;

namespace FoodRecommendation.Models.Entity;

public partial class Ingredient
{
    public int IngredientId { get; set; }

    public int RecipeId { get; set; }

    public string? Quantity { get; set; }

    public string? IngredientsText { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;
}
