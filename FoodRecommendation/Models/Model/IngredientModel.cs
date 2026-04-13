using FoodRecommendation.Models.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodRecommendation.Models.Model
{
    public class IngredientModel
    {
        public string? Quantity { get; set; }
        public string? IngredientsText { get; set; }

    }
}
