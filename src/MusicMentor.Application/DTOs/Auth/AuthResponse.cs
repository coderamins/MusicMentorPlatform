namespace MusicMentor.Application.DTOs.Auth;

public class AuthResponse
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Role { get; set; } = default!; // Teacher | Student | Admin
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
}

/// <summary>نتیجه یک عملیات که می‌تواند شکست بخورد (بدون پرتاب Exception برای خطاهای منطق کسب‌وکار)</summary>
public class ServiceResult<T>
{
    public bool Succeeded { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ServiceResult<T> Success(T data) => new() { Succeeded = true, Data = data };
    public static ServiceResult<T> Fail(params string[] errors) => new() { Succeeded = false, Errors = errors.ToList() };
}
