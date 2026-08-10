# راهنمای Workflow و API — پلتفرم MusicMentor

این سند برای تیم فرانت‌اند نوشته شده: چه Endpointی، کِی، با چه نقشی صدا زده می‌شه.

## اطلاعات پایه

- **Base URL:** `http://localhost:8080` (یا آدرس دیپلوی‌شده)
- **مستندات تعاملی:** `/swagger`
- **احراز هویت:** JWT Bearer Token. بعد از لاگین/ثبت‌نام، `accessToken` رو ذخیره کن (مثلاً در حافظه/State، نه لزوماً localStorage) و توی همه‌ی درخواست‌های محافظت‌شده این هدر رو بفرست:
  ```
  Authorization: Bearer {accessToken}
  ```
- **نقش‌ها:** `Student` (هنرآموز)، `Teacher` (استاد)، `Admin`. هر Endpoint مشخص می‌کنه به کدوم نقش (یا نقش‌ها) نیاز داره.
- **فرمت خطا:** در صورت شکست، بدنه‌ی پاسخ همیشه این شکلیه:
  ```json
  { "errors": ["پیام خطای قابل نمایش به کاربر"] }
  ```
- **فرمت لیست‌های صفحه‌بندی‌شده (Pagination):**
  ```json
  {
    "items": [ ... ],
    "page": 1,
    "pageSize": 12,
    "totalCount": 47,
    "totalPages": 4
  }
  ```

---

## ۱. جریان ثبت‌نام و ورود (مشترک)

| قدم | Endpoint | نقش لازم |
|---|---|---|
| ثبت‌نام هنرآموز | `POST /api/v1/auth/register/student` | - |
| ثبت‌نام استاد | `POST /api/v1/auth/register/teacher` | - |
| ورود | `POST /api/v1/auth/login` | - |

هر سه، در صورت موفقیت، این خروجی رو می‌دن:
```json
{
  "userId": "...",
  "firstName": "...",
  "lastName": "...",
  "role": "Student | Teacher",
  "accessToken": "...",
  "expiresAtUtc": "..."
}
```
فرانت باید `role` رو نگه داره تا بفهمه چه صفحاتی (داشبورد هنرآموز/استاد) رو نشون بده.

`login` هم با ایمیل کار می‌کنه هم با شماره موبایل (فیلد `emailOrPhone`).

---

## ۲. جریان استاد (از ثبت‌نام تا فعال شدن)

استاد بلافاصله بعد از ثبت‌نام **فعال نیست** — باید مدارک/رزومه بفرسته و ادمین تاییدش کنه.

```
۱. POST /api/v1/auth/register/teacher        → ثبت‌نام (وضعیت داخلی: PendingReview)
۲. POST /api/v1/teachers/me/resume            → آپلود رزومه (نقش: Teacher)
۳. GET  /api/v1/teachers/me/status            → چک وضعیت («در انتظار تایید»/«تایید شده»/«رد شده»)
```

### آپلود رزومه — نکات مهم برای فرانت
- Endpoint: `POST /api/v1/teachers/me/resume`
- باید `multipart/form-data` باشه (نه JSON)، با یک فیلد فایل به اسم `resume`
- فرمت مجاز: PDF, DOC, DOCX
- حداکثر حجم: ۵ مگابایت
- نمونه با `fetch`:
  ```js
  const formData = new FormData();
  formData.append("resume", fileInput.files[0]);

  await fetch("/api/v1/teachers/me/resume", {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}` }, // Content-Type رو دستی ست نکن، مرورگر خودش با boundary درست می‌سازه
    body: formData,
  });
  ```

### وضعیت‌های ممکن (`GET /api/v1/teachers/me/status`)
```json
{
  "approvalStatus": "PendingReview | Approved | Rejected",
  "resumeFileName": "resume.pdf",
  "resumeUploadedAtUtc": "...",
  "rejectionReason": "فقط وقتی Rejected باشه پر می‌شه",
  "reviewedAtUtc": "..."
}
```
**فرانت باید:** تا وقتی `approvalStatus` برابر `Approved` نشده، به استاد پیام «در انتظار تایید ادمین» نشون بده و امکانات مربوط به دریافت رزرو رو غیرفعال/مخفی نگه داره. اگه `Rejected` بود، `rejectionReason` رو نشون بده و اجازه بده دوباره رزومه آپلود کنه (که خودکار برمی‌گرده به `PendingReview`).

> فقط استادهای `Approved` توی جستجوی عمومی (`GET /api/v1/teachers`) دیده می‌شن و فقط اونا می‌تونن رزرو بپذیرن.

---

## ۳. جریان هنرآموز (جستجو → رزرو → پرداخت)

```
۱. GET  /api/v1/teachers?city=...&musicCategoryIds=1&sortBy=...   → لیست/جستجوی اساتید (عمومی، لاگین لازم نیست)
۲. GET  /api/v1/teachers/{id}                                      → جزئیات یک استاد
۳. POST /api/v1/bookings                                           → ارسال درخواست رزرو (نقش: Student)
        ↓ (منتظر تایید استاد)
۴. GET  /api/v1/bookings/mine  یا  GET /api/v1/bookings/{id}       → چک وضعیت درخواست
        ↓ (وقتی استاد approve کرد، status می‌شه AwaitingPayment)
۵. POST /api/v1/payments/zarinpal/request                          → گرفتن لینک پرداخت
۶. مرورگر کاربر رو به paymentUrl ریدایرکت کن                        → کاربر توی سایت زرین‌پال پرداخت می‌کنه
        ↓ (زرین‌پال خودش کاربر رو به callback ما برمی‌گردونه و تایید می‌کنه)
۷. GET  /api/v1/bookings/{id}                                      → status الان باید Confirmed باشه
```

### پارامترهای جستجوی اساتید (`GET /api/v1/teachers`)
همه Query String و اختیاری‌ان:

| پارامتر | نوع | توضیح |
|---|---|---|
| `city` | string | فیلتر شهر |
| `district` | string | فیلتر محله |
| `musicCategoryIds` | int[] | یک یا چند شناسه حوزه (از `/api/v1/music-categories`) |
| `minPrice` / `maxPrice` | decimal | بازه شهریه |
| `minExperienceYears` | int | حداقل سابقه |
| `search` | string | جستجوی آزاد در نام/بیوگرافی |
| `sortBy` | string | `MostPopular` (پیش‌فرض) \| `MostExperienced` \| `PriceLowToHigh` \| `PriceHighToLow` \| `Newest` |
| `page` / `pageSize` | int | صفحه‌بندی (حداکثر `pageSize`: ۵۰) |

برای پر کردن لیست چک‌باکس/دراپ‌داون حوزه‌ها، اول این رو صدا بزن:
```
GET /api/v1/music-categories   → [{ "id": 1, "name": "گیتار" }, ...]
```

### ثبت درخواست رزرو (`POST /api/v1/bookings`)
```json
{
  "teacherProfileId": "...",
  "musicCategoryId": 1,
  "sessionStartUtc": "2026-08-20T14:30:00Z",
  "durationMinutes": 60,
  "studentNote": "پیام اختیاری برای استاد"
}
```
نکته: `sessionStartUtc` باید در آینده باشه، وگرنه خطا می‌گیری.

### وضعیت‌های Booking (فیلد `status` توی پاسخ‌ها)
```
PendingTeacherApproval → (استاد approve کنه) → AwaitingPayment → (پرداخت موفق) → Confirmed
        │                                            │
        └─ (استاد reject کنه) → Rejected             └─ (لغو) → Cancelled
        └─ (لغو) → Cancelled
```
**فرانت باید** بر اساس این مقدار، دکمه‌های مناسب رو نشون بده:
- `PendingTeacherApproval`: فقط دکمه‌ی «لغو درخواست»
- `AwaitingPayment`: دکمه‌ی «پرداخت» (که کاربر رو می‌بره سراغ مرحله ۵)
- `Confirmed`: نمایش اطلاعات نهایی کلاس، بدون دکمه‌ی اکشن خاص
- `Rejected` / `Cancelled`: فقط نمایش وضعیت

### پرداخت (`POST /api/v1/payments/zarinpal/request`)
```json
// درخواست
{ "bookingId": "..." }

// پاسخ
{
  "paymentId": "...",
  "authority": "...",
  "paymentUrl": "https://sandbox.zarinpal.com/pg/StartPay/..."
}
```
فرانت باید کاربر رو مستقیم به `paymentUrl` ریدایرکت کنه (`window.location.href = paymentUrl`). بعد از پرداخت، زرین‌پال خودش کاربر رو به یه صفحه‌ی نتیجه (سمت بک‌اند، فعلاً یه HTML ساده) برمی‌گردونه. **فعلاً فرانت مستقیم توی این مرحله دخالتی نداره** — فقط باید بعد از برگشت کاربر (یا با پولینگ/رفرش صفحه)، دوباره `GET /api/v1/bookings/{id}` رو بزنه تا `status` رو چک کنه.

> نکته برای بک‌اند (نه فرانت): وقتی صفحه‌ی نتیجه‌ی زرین‌پال تبدیل به یه صفحه‌ی واقعی فرانت بشه، باید `ZarinPal:CallbackUrl` طوری تنظیم بشه که به یه route توی فرانت ریدایرکت کنه، نه صفحه‌ی HTML فعلی بک‌اند.

---

## ۴. جریان ادمین (بررسی و تایید اساتید)

```
۱. GET  /api/v1/admin/teachers?status=PendingReview   → لیست اساتید در انتظار بررسی (نقش: Admin)
۲. GET  /api/v1/admin/teachers/{id}                    → جزئیات کامل (بیوگرافی، حوزه‌ها، اطلاعات رزومه)
۳. GET  /api/v1/admin/teachers/{id}/resume              → دانلود/نمایش فایل رزومه
۴-الف. POST /api/v1/admin/teachers/{id}/approve         → تایید
۴-ب.  POST /api/v1/admin/teachers/{id}/reject           → رد (بدنه: { "reason": "..." })
```

پارامتر `status` می‌تونه `PendingReview` / `Approved` / `Rejected` / `All` باشه (اگه ندی، پیش‌فرض `PendingReview`).

`GET /api/v1/admin/teachers/{id}/resume` مستقیم بایت‌های فایل رو با `Content-Type` درست برمی‌گردونه — یعنی فرانت می‌تونه این URL رو مستقیم توی `<a href>` یا `<iframe>` (برای پیش‌نمایش PDF) بذاره، فقط باید هدر `Authorization` هم همراهش بره (پس بهتره با `fetch` بلاب بگیره و بعد `URL.createObjectURL` کنه، نه یه لینک ساده، چون تگ `<a>` نمی‌تونه هدر سفارشی بفرسته).

---

## ۵. لیست هنرجوها (فقط برای استاد/ادمین)

اگه استاد یا ادمین بخواد لیست هنرجوهای پلتفرم رو ببینه:
```
GET /api/v1/students?city=...&search=...&sortBy=Newest&page=1&pageSize=12
GET /api/v1/students/{studentProfileId}
```
این برخلاف لیست اساتید، **عمومی نیست** — چون شامل اطلاعات شخصی (شماره تماس/ایمیل هنرجو در جزئیات) می‌شه.

---

## جدول کامل Endpointها

| Method | Route | نقش لازم | توضیح |
|---|---|---|---|
| POST | `/api/v1/auth/register/student` | - | ثبت‌نام هنرآموز |
| POST | `/api/v1/auth/register/teacher` | - | ثبت‌نام استاد |
| POST | `/api/v1/auth/login` | - | ورود |
| GET | `/api/v1/music-categories` | - | لیست حوزه‌ها/سازها |
| GET | `/api/v1/teachers` | - | جستجوی عمومی اساتید (فقط Approved) |
| GET | `/api/v1/teachers/{id}` | - | جزئیات یک استاد |
| POST | `/api/v1/teachers/me/resume` | Teacher | آپلود رزومه |
| GET | `/api/v1/teachers/me/status` | Teacher | وضعیت تایید خودش |
| GET | `/api/v1/students` | Teacher, Admin | جستجوی هنرجوها |
| GET | `/api/v1/students/{id}` | Teacher, Admin | جزئیات یک هنرجو |
| POST | `/api/v1/bookings` | Student | ثبت درخواست رزرو |
| POST | `/api/v1/bookings/{id}/approve` | Teacher | تایید رزرو |
| POST | `/api/v1/bookings/{id}/reject` | Teacher | رد رزرو |
| POST | `/api/v1/bookings/{id}/cancel` | Student, Teacher | لغو رزرو |
| GET | `/api/v1/bookings/mine` | هر کاربر لاگین‌کرده | لیست رزروهای من |
| GET | `/api/v1/bookings/{id}` | طرفین رزرو | جزئیات یک رزرو |
| POST | `/api/v1/payments/zarinpal/request` | Student | ایجاد تراکنش پرداخت |
| GET | `/api/v1/payments/zarinpal/callback` | - (زرین‌پال صدا می‌زنه) | برگشت از درگاه |
| GET | `/api/v1/admin/teachers` | Admin | لیست اساتید برای بررسی |
| GET | `/api/v1/admin/teachers/{id}` | Admin | جزئیات کامل برای بررسی |
| GET | `/api/v1/admin/teachers/{id}/resume` | Admin | دانلود رزومه |
| POST | `/api/v1/admin/teachers/{id}/approve` | Admin | تایید استاد |
| POST | `/api/v1/admin/teachers/{id}/reject` | Admin | رد استاد |

---

## نکات فنی مهم برای فرانت

1. **همیشه هدر `Authorization: Bearer {token}` رو بفرست**، به‌جز برای Endpointهایی که نقش لازم ندارن (ثبت‌نام/ورود/جستجوی عمومی اساتید/callback زرین‌پال).
2. **توکن منقضی می‌شه** (`expiresAtUtc`، پیش‌فرض ۶۰ دقیقه). فعلاً Refresh Token نداریم — وقتی درخواستی ۴۰۱ برگردوند، کاربر رو به صفحه‌ی لاگین هدایت کن.
3. **تاریخ‌ها همه UTC** هستن (`SessionStartUtc`, `CreatedAtUtc`, ...) — حتماً موقع نمایش به تایم‌زون محلی کاربر تبدیلشون کن.
4. **مبالغ به تومان** هستن (نه ریال)، هم `hourlyRate` هم `priceAmount` هم مبلغ پرداخت به زرین‌پال.
5. آپلود رزومه تنها Endpointیه که `multipart/form-data` می‌خواد؛ بقیه همه JSON هستن.
