// ==========================================================================
// این دو تیکه رو به DbSeeder.cs موجودت اضافه کن:
// ==========================================================================

// ۱) این خط رو داخل متد SeedAsync اضافه کن (مثلاً همون اول، بعد از حلقه‌ی
//    ساخت Roleها و قبل از SeedTeacherAsync ها):
//
//    await SeedAdminAsync(userManager, "admin@musicmentor.ir", "09120000099", "مدیر", "سیستم");


// ۲) این متد جدید رو به کلاس DbSeeder اضافه کن (کنار SeedTeacherAsync/SeedStudentAsync):

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

        // ادمین برخلاف استاد/هنرآموز، پروفایل تخصصی (TeacherProfile/StudentProfile) نداره
        await userManager.AddToRoleAsync(user, UserRoles.Admin);
    }
