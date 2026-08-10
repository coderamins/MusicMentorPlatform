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

## رزرو کلاس و پرداخت (زرین‌پال)

| Method | Route | نقش | توضیح |
|---|---|---|---|
| POST | `/api/v1/bookings` | Student | ثبت درخواست رزرو برای یک استاد |
| POST | `/api/v1/bookings/{id}/approve` | Teacher | تایید درخواست (وضعیت → `AwaitingPayment`) |
| POST | `/api/v1/bookings/{id}/reject` | Teacher | رد درخواست |
| POST | `/api/v1/bookings/{id}/cancel` | Student/Teacher | لغو رزروی که هنوز `Confirmed` نشده |
| GET | `/api/v1/bookings/mine` | هر دو | لیست رزروهای کاربر جاری |
| GET | `/api/v1/bookings/{id}` | طرفین رزرو | جزئیات یک رزرو |
| POST | `/api/v1/payments/zarinpal/request` | Student | ایجاد تراکنش در زرین‌پال برای رزروی که `AwaitingPayment` است؛ خروجی شامل `paymentUrl` برای Redirect است |
| GET | `/api/v1/payments/zarinpal/callback` | - (عمومی) | آدرس بازگشت از درگاه؛ خودش verify را انجام می‌دهد و رزرو را `Confirmed` می‌کند |

جریان کامل:

```
Student → POST /bookings                      (PendingTeacherApproval)
Teacher → POST /bookings/{id}/approve          (AwaitingPayment)
Student → POST /payments/zarinpal/request      → paymentUrl
Student → مرورگر را به paymentUrl هدایت کن     (کاربر در سایت زرین‌پال پرداخت می‌کند)
ZarinPal → GET /payments/zarinpal/callback     (verify خودکار → Confirmed)
```

### راه‌اندازی زرین‌پال

۱. از [پنل زرین‌پال](https://next.zarinpal.com) یک مرچنت‌کد بگیرید (برای تست، حالت Sandbox نیازی به مرچنت واقعی ندارد؛ هر GUID دلخواه کار می‌کند).

۲. مقدار را در `appsettings.json` یا (برای Docker) در فایل `.env` کنار `docker-compose.yml` قرار دهید:

```
ZARINPAL_MERCHANT_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
```

۳. `ZarinPal:CallbackUrl` باید آدرسی باشد که از بیرون (از سمت کاربر/زرین‌پال) قابل دسترسی است — برای تست لوکال با Docker همان `http://localhost:8080/...` کار می‌کند، اما برای Production باید دامنه‌ی واقعی سرور باشد.

۴. `ZarinPal:Sandbox: true` تراکنش‌ها را روی `sandbox.zarinpal.com` می‌فرستد (پول واقعی جابه‌جا نمی‌شود). پیش از رفتن به Production این مقدار را `false` کنید.

> **نکته:** فعلاً Endpoint کال‌بک یک صفحه HTML ساده برمی‌گرداند. وقتی فرانت‌اند آماده شد، داخل `PaymentsController.Callback` به‌جای آن صفحه، باید کاربر با `Redirect` به صفحه‌ی نتیجه در فرانت‌اند هدایت شود (کامنت مربوطه در همان متد گذاشته شده).

## نقشه راه فازهای بعدی

1. ~~مدیریت لیست اساتید (فیلتر بر اساس شهر/محله/حوزه/شهریه/امتیاز) — جستجو و صفحه‌بندی~~ ✅ انجام شد
2. ~~رزرو کلاس + پرداخت داخل اپلیکیشن (زرین‌پال)~~ ✅ انجام شد
3. فروشگاه آلات موسیقی و لوازم جانبی (کاتالوگ، سبد خرید، سفارش)
4. سیستم امتیازدهی و نظرات هنرآموزان به استاد
5. مدیریت تقویم/بازه‌های زمانی در دسترس هر استاد (جلوگیری از رزرو دو جلسه هم‌زمان)



## مستندات API برای فرانت‌اند
راهنمای کامل Workflow و لیست Endpointها: [docs/API_Workflow_Guide.md](./docs/API_Workflow_Guide.md)