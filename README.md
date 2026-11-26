# BootCamp.Manager

پنل آموزشی مدرس برای تسک جلسه سوم بوت‌کمپ C#؛ رابط فارسی RTL، ASP.NET Core 10 با Controllers، EF Core 10 و SQLite.

این پروژه مسیر تمرین قبلی [CSharp.CRUD.Practice](https://github.com/aligholizadeh8413/CSharp.CRUD.Practice) را از Console CRUD به HTTP ادامه می‌دهد. اسلایدهای `session3.pptx` مبنای سطح آموزشی هستند. چهار لایه کوچک داریم و خبری از React، TypeScript، MediatR، AutoMapper یا Docker نیست.

## شروع روی macOS

۱. از [صفحه رسمی .NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) **SDK 10** را نصب کنید: برای Apple Silicon نسخه Arm64، برای مک Intel نسخه x64. Runtime به‌تنهایی کافی نیست. `global.json` اجازه استفاده از SDK پایدار 10.0 با feature band جدیدتر را می‌دهد؛ اگر فقط SDK 11 دارید، SDK 10 را کنار آن نصب کنید.

۲. ZIP را باز کنید و در Terminal وارد پوشه‌ای شوید که `BootCamp.Manager.sln` در آن است:

```bash
cd BootCamp.Manager
dotnet --list-sdks
dotnet tool restore
dotnet restore --disable-parallel -p:BuildInParallel=false
dotnet build -m:1
```

۳. Migration را اعمال و برنامه را اجرا کنید:

```bash
dotnet ef database update \
  --project src/BootCamp.Manager.Infrastructure \
  --startup-project src/BootCamp.Manager.WebApi \
  --no-build

dotnet run --no-build --project src/BootCamp.Manager.WebApi
```

پنل: **http://localhost:5083** — مثال MVC: **http://localhost:5083/learning/mvc**

اولین اجرا دیتابیس خالی دارد. ابتدا مدرس و دانشجو اضافه کنید، سپس جلسه بسازید. برنامه هنگام شروع نیز `MigrateAsync` را اجرا می‌کند؛ بنابراین دستور `database update` برای مشاهده و تمرین فرایند است و اجرای دوباره‌اش مشکلی ندارد. این تصمیم برای تمرین محلی است.

برای پر کردن دیتابیس خالی با نمونه‌های ساختگی، در ترمینال دوم (با Python 3 نصب‌شده) اجرا کنید:

```bash
python3 scripts/seed-demo.py
```

داده نمونه واقعاً با POST وارد SQLite می‌شود؛ با داده مرورگری `docs` متفاوت است. اسکریپت فقط وقتی تمام جدول‌های اصلی خالی باشند شروع می‌کند. برنامه به login نیاز ندارد و برای اجرای محلی این تمرین تنظیم شده است.

## قابلیت‌ها و قواعد

- CRUD دانشجو، مدرس، جلسه و ارزیابی؛ حضور و غیاب قابل ثبت و ویرایش.
- تسک هر جلسه برای جلسه بعد و درصد پیشرفت درس ۰ تا ۱۰۰.
- شرکت‌کنندگان هر جلسه هنگام ساخت انتخاب و ذخیره می‌شوند. افزودن یا غیرفعال کردن دانشجو، آمار جلسه قبلی را عوض نمی‌کند.
- درصد حضور = حاضرها ÷ کل شرکت‌کنندگان همان جلسه × ۱۰۰. وضعیت ثبت‌نشده از حاضر جدا است؛ جلسه خالی `null` و در UI «—» نمایش می‌دهد.
- ارزیابی ۰ تا ۱۰۰ با توضیحات و مدرس ارزیاب؛ حداکثر یک ارزیابی برای هر دانشجو در هر جلسه.
- فرد دارای سابقه حذف نمی‌شود (`409`)؛ غیرفعالش کنید. جلسه با حذف، حضور و ارزیابی‌های خودش را هم حذف می‌کند.
- فرم‌ها Modal؛ صفحه‌بندی کارت‌ها؛ فونت Rokh از فایل ارسالی؛ SVG؛ بدون ایموجی.
- تاریخ در ورودی استاندارد مرورگر میلادی است؛ نمایش با `Intl` شمسی و فارسی است.

## راهنمای مطالعه

| فایل | محتوای آموزشی |
|---|---|
| [معماری و دیاگرام‌ها](documentation/01-architecture.md) | ساختار پروژه، وابستگی لایه‌ها، ERD و Request Flow |
| [آموزش قدم‌به‌قدم](documentation/02-learning-guide.md) | مسئولیت فایل‌ها، EF، DI، DTO، Controller و مقایسه با FastAPI |
| [مرجع کامل API](documentation/03-api.md) | همه مسیرها، JSON و خطاها |
| [Middleware، Filter و فرم‌ها](documentation/04-web-concepts.md) | تفاوت مفاهیم و مثال MVC قابل تست |
| [اجرا و Migration](documentation/05-running.md) | macOS، SQL، GitHub Pages و عیب‌یابی |
| [گزارش تست واقعی](documentation/06-test-report.md) | نتایج اجرا و محدودیت‌های تأیید |
| [BootCamp.Manager.http](BootCamp.Manager.http) | توالی CRUD با HTTP Client |
| [schema.sql](documentation/schema.sql) | SQL واقعی تولیدشده از Migration |

پیشنهاد مطالعه: `Student` → `StudentRequest` → `StudentsController` → `StudentService` → `StudentRepository` → `BootcampDbContext` → `Program.cs` → `wwwroot/js/data.js`.

## تست مجدد

```bash
dotnet build -m:1
python3 tests/smoke.py
node tests/demo.mjs
```

تست Python سرور واقعی را روی یک پورت آزاد بالا می‌آورد، یک دیتابیس موقت می‌سازد، CRUD و restart را بررسی می‌کند و دیتابیس موقت را پاک می‌کند. Node فقط برای تست اختیاری JavaScript است؛ اجرای خود برنامه به Node یا npm نیاز ندارد.

## دموی GitHub Pages

`docs/` یک نسخه مستقل است: درخواست `/api` نمی‌فرستد؛ داده ساختگی در `localStorage` همین مرورگر نگه‌داری می‌شود. بالای صفحه عبارت «به بک‌اند متصل نیست» دیده می‌شود. دکمه بازنشانی، داده نمونه را برمی‌گرداند.

```bash
python3 -m http.server 8080 --directory docs
```

سپس `http://localhost:8080` را باز کنید. فایل را با `file://` باز نکنید چون JavaScript module به HTTP نیاز دارد. برای انتشار: فایل‌ها را در [مخزن BootCamp.Manager](https://github.com/aligholizadeh8413/BootCamp.Manager) قرار دهید و در Settings → Pages، گزینه Deploy from a branch، شاخه `main` و پوشه `/docs` را انتخاب کنید. آماده بودن پوشه به معنی منتشر شدن Pages نیست؛ این بسته به مخزن push نشده است.

## تصمیم‌های ساده‌سازی

Pagination در مرورگر انجام می‌شود؛ API فهرست کامل را می‌دهد. داده کمِ یک بوت‌کمپ هدف این تمرین است. Serviceها مستقیماً با DI ثبت شده‌اند، Repositoryها interface دارند؛ برای هر کلاس interface اضافی یا UnitOfWork جدا نساخته‌ایم چون DbContext همین نقش ذخیره‌سازی را دارد. فایل‌های Migration تولید EF هستند و عمداً از سورس حذف نشده‌اند.
