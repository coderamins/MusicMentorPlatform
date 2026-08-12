using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicMentor.Domain.Entities;
using MusicMentor.Domain.Enums;
using MusicMentor.Infrastructure.Data;

namespace MusicMentor.Infrastructure.Seed;

public static class ApplicationUserSeed
{
    private const string SeedPassword = "Adm@in123";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var role in UserRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
        await SeedAdminAsync(userManager,
            email: "ramins@gmail.com",
            phone: "09930701955",
            firstName: "رامین",
            lastName: "صلحی"); //کاربر ادمین 


        await SeedTeacherAsync(userManager, db,
            email: "mohsenkia70@gmail.com",
            phone: "09930701955",
            firstName: "محسن",
            lastName: "ابراهیمی کیا",
            city: "تهران",
            district: "ونک",
            bio: "۱۰ سال سابقه تدریس گیتار کلاسیک و پاپ، مناسب مبتدی تا پیشرفته.",
            yearsOfExperience: 10,
            hourlyRate: 500_000m,
            isVerified: true,
            ratingAverage: 4.7,
            ratingCount: 32,
            musicCategoryIds: new[] { 1, 6 }); // گیتار، آواز

        await SeedTeacherAsync(userManager, db,
            email: "sara.ahmadi@example.com",
            phone: "09120000002",
            firstName: "سارا",
            lastName: "احمدی",
            city: "تهران",
            district: "نیاوران",
            bio: "مدرس پیانو با روش کودک‌محور و بزرگسال، برگزارکننده چند کنسرت دانشجویی.",
            yearsOfExperience: 6,
            hourlyRate: 350_000m,
            isVerified: true,
            ratingAverage: 4.5,
            ratingCount: 18,
            musicCategoryIds: new[] { 2 }); // پیانو

        await SeedTeacherAsync(userManager, db,
            email: "reza.karimi@example.com",
            phone: "09120000003",
            firstName: "رضا",
            lastName: "کریمی",
            city: "شیراز",
            district: null,
            bio: "نوازنده و مدرس ویولن و سه‌تار، فارغ‌التحصیل موسیقی دانشگاه هنر.",
            yearsOfExperience: 15,
            hourlyRate: 700_000m,
            isVerified: false,
            ratingAverage: 0,
            ratingCount: 0,
            musicCategoryIds: new[] { 3, 5 }); // ویولن، تار و سه‌تار

        await SeedStudentAsync(userManager, db,
            email: "mina.hosseini@example.com",
            phone: "09120000011",
            firstName: "مینا",
            lastName: "حسینی",
            city: "تهران",
            district: "ونک",
            learningGoal: "یادگیری گیتار برای اجرای دورهمی خانوادگی");

        await SeedStudentAsync(userManager, db,
            email: "amir.rostami@example.com",
            phone: "09120000012",
            firstName: "امیر",
            lastName: "رستمی",
            city: "شیراز",
            district: null,
            learningGoal: null);

        await db.SaveChangesAsync();
    }

    private static async Task SeedTeacherAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        string email,
        string phone,
        string firstName,
        string lastName,
        string city,
        string? district,
        string bio,
        int yearsOfExperience,
        decimal hourlyRate,
        bool isVerified,
        double ratingAverage,
        int ratingCount,
        int[] musicCategoryIds)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return; // قبلاً Seed شده

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = phone,
            FirstName = firstName,
            LastName = lastName,
            City = city,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(user, SeedPassword);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(
                $"خطا در ساخت کاربر Seed «{email}»: " +
                string.Join(", ", createResult.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, UserRoles.Teacher);

        var validCategoryIds = await db.MusicCategories
            .Where(c => musicCategoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        var teacherProfile = new TeacherProfile
        {
            UserId = user.Id,
            City = city,
            District = district,
            Bio = bio,
            YearsOfExperience = yearsOfExperience,
            HourlyRate = hourlyRate,
            ApprovalStatus = isVerified ? TeacherApprovalStatus.Approved : TeacherApprovalStatus.PendingReview,
            RatingAverage = ratingAverage,
            RatingCount = ratingCount,
        };

        foreach (var categoryId in validCategoryIds)
        {
            teacherProfile.Categories.Add(new TeacherMusicCategory
            {
                TeacherProfileId = teacherProfile.Id,
                MusicCategoryId = categoryId,
            });
        }

        db.TeacherProfiles.Add(teacherProfile);
    }

    private static async Task SeedStudentAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        string email,
        string phone,
        string firstName,
        string lastName,
        string city,
        string? district,
        string? learningGoal)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return; // قبلاً Seed شده

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = phone,
            FirstName = firstName,
            LastName = lastName,
            City = city,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(user, SeedPassword);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(
                $"خطا در ساخت کاربر Seed «{email}»: " +
                string.Join(", ", createResult.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, UserRoles.Student);

        db.StudentProfiles.Add(new StudentProfile
        {
            UserId = user.Id,
            City = city,
            District = district,
            LearningGoal = learningGoal,
        });
    }


    private static async Task SeedAdminAsync(
    UserManager<ApplicationUser> userManager,
    string email,
    string phone,
    string firstName,
    string lastName)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return; // قبلاً Seed شده

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = phone,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(user, SeedPassword);
        if (!createResult.Succeeded)
            throw new InvalidOperationException(
                $"خطا در ساخت کاربر Seed «{email}»: " +
                string.Join(", ", createResult.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, UserRoles.Admin);
    }

}