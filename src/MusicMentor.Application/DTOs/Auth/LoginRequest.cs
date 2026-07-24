namespace MusicMentor.Application.DTOs.Auth;

public class LoginRequest
{
    /// <summary>ایمیل یا شماره موبایل ثبت‌شده</summary>
    public string EmailOrPhone { get; set; } = default!;
    public string Password { get; set; } = default!;
}
