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

- .NET 8 SDK
- Docker (برای اجرای PostgreSQL) یا یک نمونه PostgreSQL در دسترس

## راه‌اندازی

```bash
# ۱. بالا آوردن دیتابیس
docker compose up -d

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
