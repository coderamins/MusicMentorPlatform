using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicMentor.Application.DTOs.Auth;
using MusicMentor.Application.Interfaces;
using MusicMentor.Domain.Entities;
using MusicMentor.Domain.Enums;
using MusicMentor.Infrastructure.Data;

namespace MusicMentor.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ApplicationDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ApplicationDbContext db,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterStudentAsync(RegisterStudentRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return ServiceResult<AuthResponse>.Fail("کاربری با این ایمیل قبلاً ثبت‌نام کرده است.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            City = request.City,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return ServiceResult<AuthResponse>.Fail(createResult.Errors.Select(e => e.Description).ToArray());

        await EnsureRoleExistsAsync(UserRoles.Student);
        await _userManager.AddToRoleAsync(user, UserRoles.Student);

        var studentProfile = new StudentProfile
        {
            UserId = user.Id,
            City = request.City,
            District = request.District,
            LearningGoal = request.LearningGoal,
        };
        _db.StudentProfiles.Add(studentProfile);
        await _db.SaveChangesAsync();

        return await BuildAuthResponseAsync(user);
    }

    public async Task<ServiceResult<AuthResponse>> RegisterTeacherAsync(RegisterTeacherRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return ServiceResult<AuthResponse>.Fail("کاربری با این ایمیل قبلاً ثبت‌نام کرده است.");

        if (request.HourlyRate < 0)
            return ServiceResult<AuthResponse>.Fail("شهریه نمی‌تواند منفی باشد.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            City = request.City,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return ServiceResult<AuthResponse>.Fail(createResult.Errors.Select(e => e.Description).ToArray());

        await EnsureRoleExistsAsync(UserRoles.Teacher);
        await _userManager.AddToRoleAsync(user, UserRoles.Teacher);

        var teacherProfile = new TeacherProfile
        {
            UserId = user.Id,
            City = request.City,
            District = request.District,
            Bio = request.Bio,
            YearsOfExperience = request.YearsOfExperience,
            HourlyRate = request.HourlyRate,
        };

        if (request.MusicCategoryIds.Count > 0)
        {
            var validCategoryIds = await _db.MusicCategories
                .Where(c => request.MusicCategoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var categoryId in validCategoryIds)
            {
                teacherProfile.Categories.Add(new TeacherMusicCategory
                {
                    TeacherProfileId = teacherProfile.Id,
                    MusicCategoryId = categoryId,
                });
            }
        }

        _db.TeacherProfiles.Add(teacherProfile);
        await _db.SaveChangesAsync();

        return await BuildAuthResponseAsync(user);
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.EmailOrPhone)
                   ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.EmailOrPhone);

        if (user is null || !user.IsActive)
            return ServiceResult<AuthResponse>.Fail("ایمیل/موبایل یا رمز عبور نادرست است.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return ServiceResult<AuthResponse>.Fail("ایمیل/موبایل یا رمز عبور نادرست است.");

        return await BuildAuthResponseAsync(user);
    }

    private async Task<ServiceResult<AuthResponse>> BuildAuthResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAtUtc) = _tokenService.GenerateAccessToken(user, roles);

        var response = new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? string.Empty,
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
        };

        return ServiceResult<AuthResponse>.Success(response);
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
    }
}
