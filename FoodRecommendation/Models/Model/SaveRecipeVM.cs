
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class SavedRecipeVM
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? ImageUrl { get; set; }


    // thêm mới
    public string Author { get; set; }
    public int? Cooktime { get; set; }
    public string RoleUser { get; set; }
}