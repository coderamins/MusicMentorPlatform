using MusicMentor.Domain.Entities;

namespace MusicMentor.Application.Interfaces;

public interface ITokenService
{
    /// <summary>تولید JWT برای کاربر بر اساس نقش‌هایش</summary>
    (string token, DateTime expiresAtUtc) GenerateAccessToken(ApplicationUser user, IList<string> roles);
}
