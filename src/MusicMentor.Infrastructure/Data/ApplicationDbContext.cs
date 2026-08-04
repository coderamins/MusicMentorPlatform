using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicMentor.Domain.Entities;

namespace MusicMentor.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<MusicCategory> MusicCategories => Set<MusicCategory>();
    public DbSet<TeacherMusicCategory> TeacherMusicCategories => Set<TeacherMusicCategory>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // نگاشت جدول‌های Identity به نام‌های دلخواه (اختیاری ولی خواناتر)
        builder.Entity<ApplicationUser>(b => b.ToTable("Users"));
        builder.Entity<IdentityRole<Guid>>(b => b.ToTable("Roles"));
        builder.Entity<IdentityUserRole<Guid>>(b => b.ToTable("UserRoles"));
        builder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("UserClaims"));
        builder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("UserLogins"));
        builder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("UserTokens"));
        builder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("RoleClaims"));

        // TeacherProfile <-> ApplicationUser (1 to 1)
        builder.Entity<TeacherProfile>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.HourlyRate).HasColumnType("numeric(12,2)");
            b.HasOne(t => t.User)
                .WithOne(u => u.TeacherProfile)
                .HasForeignKey<TeacherProfile>(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(t => t.UserId).IsUnique();
        });

        // StudentProfile <-> ApplicationUser (1 to 1)
        builder.Entity<StudentProfile>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasOne(s => s.User)
                .WithOne(u => u.StudentProfile)
                .HasForeignKey<StudentProfile>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(s => s.UserId).IsUnique();
        });

        // MusicCategory lookup table
        builder.Entity<MusicCategory>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(c => c.Name).IsUnique();
        });

        // TeacherMusicCategory many-to-many join
        builder.Entity<TeacherMusicCategory>(b =>
        {
            b.HasKey(tc => new { tc.TeacherProfileId, tc.MusicCategoryId });

            b.HasOne(tc => tc.TeacherProfile)
                .WithMany(t => t.Categories)
                .HasForeignKey(tc => tc.TeacherProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(tc => tc.MusicCategory)
                .WithMany(c => c.TeacherCategories)
                .HasForeignKey(tc => tc.MusicCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Booking
        builder.Entity<Booking>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.PriceAmount).HasColumnType("numeric(12,2)");
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

            b.HasOne(x => x.StudentProfile)
                .WithMany()
                .HasForeignKey(x => x.StudentProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.TeacherProfile)
                .WithMany()
                .HasForeignKey(x => x.TeacherProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.MusicCategory)
                .WithMany()
                .HasForeignKey(x => x.MusicCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.StudentProfileId);
            b.HasIndex(x => x.TeacherProfileId);
            b.HasIndex(x => x.Status);
        });

        // Payment
        builder.Entity<Payment>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Amount).HasColumnType("numeric(12,2)");
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            b.Property(x => x.Authority).HasMaxLength(64);
            b.Property(x => x.RefId).HasMaxLength(64);

            b.HasOne(x => x.Booking)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Authority شناسه‌ای است که Callback زرین‌پال با آن Payment مربوطه را پیدا می‌کند
            b.HasIndex(x => x.Authority).IsUnique();
        });

        // Seed اولیه چند حوزه رایج (اختیاری - قابل مدیریت بعداً توسط ادمین)
        builder.Entity<MusicCategory>().HasData(
            new MusicCategory { Id = 1, Name = "گیتار" },
            new MusicCategory { Id = 2, Name = "پیانو" },
            new MusicCategory { Id = 3, Name = "ویولن" },
            new MusicCategory { Id = 4, Name = "سنتور" },
            new MusicCategory { Id = 5, Name = "تار و سه‌تار" },
            new MusicCategory { Id = 6, Name = "آواز" },
            new MusicCategory { Id = 7, Name = "درامز و پرکاشن" }
        );
    }
}
