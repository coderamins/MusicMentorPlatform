using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MusicMentor.Api.Common;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// شناسه کاربر جاری را از توکن JWT استخراج می‌کند.
    /// چون ASP.NET Core به‌صورت پیش‌فرض claim با نام "sub" را به ClaimTypes.NameIdentifier نگاشت می‌دهد،
    /// هر دو حالت را برای اطمینان بررسی می‌کنیم.
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            throw new InvalidOperationException("شناسه کاربر در توکن معتبر یافت نشد.");

        return userId;
    }
}
