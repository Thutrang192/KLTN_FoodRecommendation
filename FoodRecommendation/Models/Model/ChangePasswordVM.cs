using System.ComponentModel.DataAnnotations;

public class ChangePasswordVM
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; }
}