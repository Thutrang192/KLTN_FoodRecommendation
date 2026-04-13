using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodRecommendation.Models.Entity;

public partial class Account
{
    public int UserId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Tên đăng nhập không được để trống")]
    public string Username { get; set; } = string.Empty;


    [DataType(DataType.EmailAddress)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email không được để trống")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                            @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                            @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                            ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? RoleUser { get; set; }

    public bool? IsActivated { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}
