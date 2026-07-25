# syntax=docker/dockerfile:1

# ---------- مرحله ۱: Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ابتدا فقط فایل‌های csproj کپی می‌شوند تا لایه‌ی restore کش شود
# و با تغییر کد، مجبور به restore دوباره‌ی همه‌ی پکیج‌ها نباشیم.
COPY MusicMentorPlatform.sln ./
COPY src/MusicMentor.Domain/MusicMentor.Domain.csproj src/MusicMentor.Domain/
COPY src/MusicMentor.Application/MusicMentor.Application.csproj src/MusicMentor.Application/
COPY src/MusicMentor.Infrastructure/MusicMentor.Infrastructure.csproj src/MusicMentor.Infrastructure/
COPY src/MusicMentor.Api/MusicMentor.Api.csproj src/MusicMentor.Api/

RUN dotnet restore src/MusicMentor.Api/MusicMentor.Api.csproj

# حالا بقیه‌ی سورس کپی و publish می‌شود
COPY src/ src/
RUN dotnet publish src/MusicMentor.Api/MusicMentor.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---------- مرحله ۲: Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# اجرا با کاربر غیر-root (best practice امنیتی)
RUN useradd --uid 1000 --create-home appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MusicMentor.Api.dll"]
