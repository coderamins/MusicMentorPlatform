namespace MusicMentor.Infrastructure.Services;

/// <summary>از appsettings.json (بخش "Jwt") پر می‌شود</summary>
public class JwtSettings
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public int AccessTokenMinutes { get; set; } = 60;
}
