
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class UpdateProfileVM
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string? Email { get; set; }

    public string? AvatarUrl { get; set; }

    public IFormFile? AvatarFile { get; set; }

    public bool RemoveAvatar { get; set; }
}