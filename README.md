# MusicMentor Platform — بک‌اند

پلتفرم ارتباط استاد و هنرآموز موسیقی. این ریپازیتوری فاز اول پروژه است: **سرویس احراز هویت (ثبت‌نام/ورود) استاد و هنرآموز**.

## معماری

پروژه به ۴ لایه تقسیم شده تا در فازهای بعدی (پیشنهاد کلاس، رزرو، پرداخت، فروشگاه آلات موسیقی) به‌راحتی قابل توسعه باشد:

```
src/
  MusicMentor.Domain          -> Entityهای اصلی (User, TeacherProfile, StudentProfile, MusicCategory)
  MusicMentor.Application     -> DTOها و اینترفیس‌های سرویس (مستقل از EF/Identity)
  MusicMentor.Infrastructure  -> DbContext (Postgres/EF Core)، Identity، JWT، پیاده‌سازی سرویس‌ها
  MusicMentor.Api             -> Controllerها، Program.cs، تنظیمات
```

## پیش‌نیازها

- Docker و Docker Compose (ساده‌ترین راه اجرا)
- یا به‌صورت جایگزین: .NET 8 SDK + یک نمونه PostgreSQL برای اجرای محلی بدون Docker

## راه‌اندازی با Docker (پیشنهادی)

با یک دستور، هم دیتابیس Postgres و هم سرویس Api بالا می‌آید. Migrationها هم به‌صورت خودکار هنگام start شدن Api روی دیتابیس اعمال می‌شوند (نیازی به اجرای دستی `dotnet ef` نیست):

```bash
docker compose up -d --build
```

بعد از چند ثانیه:

- Swagger: http://localhost:8080/swagger
- Postgres روی پورت `5432` هاست در دسترس است (برای اتصال با ابزارهایی مثل DBeaver/pgAdmin)

برای دیدن لاگ‌ها:

```bash
docker compose logs -f api
```

برای متوقف کردن:

```bash
docker compose down          # کانتینرها متوقف می‌شوند، دیتای دیتابیس می‌ماند
docker compose down -v       # به‌همراه پاک کردن Volume دیتابیس (ریست کامل)
```

> **نکته امنیتی:** مقادیر `Jwt__SecretKey` و پسورد Postgres داخل `docker-compose.yml` فقط برای توسعه هستند. پیش از انتشار (Production) حتماً آن‌ها را با مقادیر واقعی از طریق فایل `.env` (که در گیت commit نمی‌شود) یا یک secret manager جایگزین کنید.

### ساخت و اجرای مستقل ایمیج Api (بدون Compose)

اگر فقط می‌خواهید ایمیج سرویس Api را جدا بسازید (مثلاً برای push به یک Registry):

```bash
docker build -t musicmentor-api:latest .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=<postgres-host>;Port=5432;Database=musicmentor;Username=postgres;Password=postgres" \
  -e Jwt__SecretKey="یک-secret-طولانی-و-تصادفی" \
  musicmentor-api:latest
```

## راه‌اندازی محلی بدون Docker (اختیاری)

```bash
# ۱. بالا آوردن فقط دیتابیس با Docker
docker compose up -d postgres

# ۲. نصب ابزار EF Core (یک‌بار در سیستم)
dotnet tool install --global dotnet-ef

# ۳. ساخت اولین Migration
cd src/MusicMentor.Api
dotnet ef migrations add InitialCreate --project ../MusicMentor.Infrastructure --startup-project .

# ۴. اعمال Migration روی دیتابیس
dotnet ef database update --project ../MusicMentor.Infrastructure --startup-project .

# ۵. اجرای پروژه
dotnet run
```

سپس مستندات Swagger روی آدرس `https://localhost:7001/swagger` در دسترس است.

> **نکته:** مقدار `Jwt:SecretKey` در `appsettings.json` فقط برای توسعه است. حتماً پیش از انتشار (Production) آن را با `dotnet user-secrets` یا Environment Variable جایگزین کنید.

## Endpointهای فاز اول (احراز هویت)

| Method | Route                        | توضیح                              |
|--------|------------------------------|-------------------------------------|
| POST   | `/api/v1/auth/register/student` | ثبت‌نام هنرآموز                    |
| POST   | `/api/v1/auth/register/teacher` | ثبت‌نام استاد (همراه سابقه، شهریه، حوزه تدریس) |
| POST   | `/api/v1/auth/login`            | ورود مشترک (با ایمیل یا موبایل)   |

## Endpointهای فاز دوم (لیست و جستجوی اساتید)

| Method | Route                          | توضیح                                                            |
|--------|--------------------------------|-------------------------------------------------------------------|
| GET    | `/api/v1/teachers`             | جستجو/لیست اساتید با فیلتر و صفحه‌بندی (پارامترها را پایین ببینید) |
| GET    | `/api/v1/teachers/{id}`        | پروفایل کامل یک استاد                                             |
| GET    | `/api/v1/music-categories`     | لیست حوزه‌ها/سازها برای پر کردن فیلتر جستجو                       |

پارامترهای QueryString برای `GET /api/v1/teachers`:

| پارامتر            | نوع     | توضیح                                                         |
|---------------------|---------|-----------------------------------------------------------------|
| `search`            | string  | جستجو در نام و بیوگرافی استاد                                  |
| `city`              | string  | فیلتر بر اساس شهر                                              |
| `district`          | string  | فیلتر بر اساس محله                                              |
| `musicCategoryIds`  | int[]   | یک یا چند شناسه حوزه (مثلاً `?musicCategoryIds=1&musicCategoryIds=6`) |
| `minPrice`/`maxPrice` | decimal | بازه شهریه                                                    |
| `minExperienceYears`| int     | حداقل سابقه تدریس (سال)                                        |
| `onlyVerified`      | bool    | فقط اساتید تاییدشده                                             |
| `sortBy`            | enum    | `MostPopular` \| `MostExperienced` \| `PriceLowToHigh` \| `PriceHighToLow` \| `Newest` |
| `page` / `pageSize`  | int     | صفحه‌بندی (پیش‌فرض: صفحه ۱، سایز ۱۲، حداکثر سایز ۵۰)           |

نمونه:

```
GET /api/v1/teachers?city=تهران&musicCategoryIds=1&minPrice=200000&maxPrice=600000&sortBy=MostPopular&page=1&pageSize=12
```

پاسخ به‌صورت `PagedResult` است:

```json
{
  "items": [
    {
      "teacherProfileId": "…",
      "userId": "…",
      "fullName": "علی محمدی",
      "city": "تهران",
      "district": "ونک",
      "yearsOfExperience": 10,
      "hourlyRate": 500000,
      "ratingAverage": 4.7,
      "ratingCount": 32,
      "isVerified": true,
      "bioShort": "۱۰ سال سابقه تدریس گیتار کلاسیک…",
      "categories": ["گیتار", "آواز"]
    }
  ],
  "page": 1,
  "pageSize": 12,
  "totalCount": 45,
  "totalPages": 4
}
```

نمونه بدنه درخواست ثبت‌نام استاد:

```json
{
  "firstName": "علی",
  "lastName": "محمدی",
  "email": "ali@example.com",
  "phoneNumber": "09120000000",
  "password": "P@ssw0rd123",
  "city": "تهران",
  "district": "ونک",
  "bio": "۱۰ سال سابقه تدریس گیتار کلاسیک",
  "yearsOfExperience": 10,
  "hourlyRate": 500000,
  "musicCategoryIds": [1, 6]
}
```

پاسخ موفق شامل `accessToken` (JWT) است که باید در درخواست‌های بعدی به‌صورت هدر زیر ارسال شود:

```
Authorization: Bearer {accessToken}
```

## نقشه راه فازهای بعدی

1. ~~مدیریت لیست اساتید (فیلتر بر اساس شهر/محله/حوزه/شهریه/امتیاز) — جستجو و صفحه‌بندی~~ ✅ انجام شد
2. سیستم پیشنهاد کلاس از هنرآموز به استاد + پذیرش/رد توسط استاد
3. رزرو کلاس و مدیریت تقویم زمانی استاد
4. درگاه پرداخت داخل اپلیکیشن
5. فروشگاه آلات موسیقی و لوازم جانبی (کاتالوگ، سبد خرید، سفارش)
6. سیستم امتیازدهی و نظرات هنرآموزان به استاد
