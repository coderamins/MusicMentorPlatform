using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MusicMentor.Infrastructure;
using MusicMentor.Infrastructure.Data;
using MusicMentor.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MusicMentor API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "JWT را به صورت: Bearer {token} وارد کنید",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() },
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

// لایه Infrastructure: DbContext (Postgres) + Identity + JWT + سرویس‌های Auth
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// اعمال خودکار Migrationهای در انتظار روی دیتابیس هنگام بالا آمدن (مناسب برای اجرای داخل Docker/Compose).
// در محیط تولید با ترافیک بالا، بهتر است این کار به یک مرحله جداگانه CI/CD منتقل شود.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
        await ApplicationUserSeed.SeedAsync(scope.ServiceProvider);
}

// --- Middleware pipeline ---
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
