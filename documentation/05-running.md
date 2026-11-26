# اجرا، نگه‌داری و رفع خطا

## macOS

SDK 10 را برای معماری مک نصب کنید. `dotnet --list-sdks` باید یک نسخه 10.0 پایدار نشان دهد. تمام دستورها از ریشه solution اجرا می‌شوند. SQLite به نصب SQL Server یا سرویس جدا احتیاج ندارد؛ Provider فایل DB را ایجاد می‌کند.

```bash
dotnet tool restore
dotnet restore --disable-parallel -p:BuildInParallel=false
dotnet build -m:1
dotnet ef database update --project src/BootCamp.Manager.Infrastructure --startup-project src/BootCamp.Manager.WebApi --no-build
dotnet run --project src/BootCamp.Manager.WebApi --no-build
```

`-m:1` صرفاً build را تک‌پردازه می‌کند؛ برای محیط‌های با محدودیت اجرای موازی مفید است و معماری برنامه را تغییر نمی‌دهد. نسخه packageهای EF و ابزار ef هر دو 10.0.14 هستند. این‌ها نسخه‌های pin‌شده این تحویل‌اند، نه ادعای آخرین patch موجود.

DB پیش‌فرض در `src/BootCamp.Manager.WebApi/App_Data/bootcamp.db` است. مسیر از ContentRoot ساخته می‌شود، نه پوشه اتفاقی Terminal. `bin/obj/App_Data` وارد git یا ZIP تحویل نشده‌اند. اگر DB را پاک کنید، اطلاعات واقعی از بین می‌روند؛ برای تغییر schema معمولی Migration جدید بسازید.

اتصال سفارشی اختیاری:

```bash
ConnectionStrings__Bootcamp='Data Source=/tmp/bootcamp-practice.db;Foreign Keys=True' \
  dotnet run --project src/BootCamp.Manager.WebApi --no-build
```

پوشه والد مسیر سفارشی باید موجود باشد. برای تغییر پورت:

```bash
dotnet run --project src/BootCamp.Manager.WebApi --no-build --no-launch-profile --urls http://localhost:5090
```

فرانت‌اند چون آدرس نسبی `/api` دارد، روی همین پورت هم کار می‌کند. HTTP محلی برای سادگی انتخاب شده و HTTPS redirection ندارد؛ نیاز نیست certificate نصب کنید.

## تغییر مدل و Migration

مثال تمرین: یک property به Student اضافه کنید، DTO/Service/UI را متناسب به‌روزرسانی کنید و سپس:

```bash
dotnet build -m:1
dotnet ef migrations add AddStudentPhone \
  --project src/BootCamp.Manager.Infrastructure \
  --startup-project src/BootCamp.Manager.WebApi --no-build
dotnet build -m:1
dotnet ef database update \
  --project src/BootCamp.Manager.Infrastructure \
  --startup-project src/BootCamp.Manager.WebApi --no-build
```

Build دوم لازم است چون Migration جدید باید کامپایل شود. فایل Migration را قبل از apply بخوانید. ModelSnapshot را دستی تغییر ندهید. `EnsureCreated` را کنار Migrate اضافه نکنید.

برای دیدن SQL:

```bash
dotnet ef migrations script \
  --project src/BootCamp.Manager.Infrastructure \
  --startup-project src/BootCamp.Manager.WebApi \
  --no-build --output documentation/schema.sql
```

SQLite برای بعضی تغییرات جدول نیاز به بازسازی جدول دارد و script idempotent به سبک SQL Server ندارد؛ فعلاً از `database update` برای دیتابیس محلی استفاده کنید. [محدودیت‌های رسمی SQLite Provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations).

## لاگ‌ها

در Terminal هر درخواست خطی مثل `HTTP PUT /api/students/1 => 204 ...` دارد. Filter نیز نام Action و شروع/پایان را می‌نویسد. برای دیدن SQL در `appsettings.json` مقدار `Microsoft.EntityFrameworkCore.Database.Command` را از Warning به Information تغییر دهید و برنامه را restart کنید. لاگ را با داده واقعی حساس به سرویس‌های بیرونی نفرستید؛ برای تمرین داده ساختگی کافی است.

## GitHub و Pages

بسته برای مخزن مقصد آماده است و README موجود مخزن هنگام طراحی بررسی شده است. از داخل checkout مخزن، فایل‌های بسته را کپی و diff را بررسی کنید؛ `.git` را با چیزی جایگزین نکنید. مراحل اختیاری commit از checkout خودتان:

```bash
git status
git add .
git commit -m "Add educational ASP.NET Core bootcamp manager"
git push
```

در Settings → Pages، Deploy from a branch → main → `/docs` را انتخاب کنید. فقط فایل‌های `docs/` سرو می‌شوند. GitHub Pages نمی‌تواند ASP.NET یا SQLite سرور را اجرا کند. URL مورد انتظار مخزن طبق README قبلی `https://aligholizadeh8413.github.io/BootCamp.Manager/` است؛ انتشار آن در این تحویل انجام نشده است.

وقتی UI اصلی را تغییر دادید:

```bash
python3 scripts/sync-demo.py
```

این اسکریپت `docs/js/data.js` را بازنویسی نمی‌کند. همه assetهای دمو relative هستند تا زیر مسیر `/BootCamp.Manager/` کار کنند. برای اجرای local دمو از HTTP server استفاده کنید، نه file://.

## عیب‌یابی

| مشکل | بررسی |
|---|---|
| SDK مناسب پیدا نشد | SDK 10 را نصب کنید؛ global.json را برای پنهان کردن مشکل حذف نکنید |
| NuGet restore ناموفق | اتصال اینترنت به NuGet و متن خطا را بررسی کنید؛ `dotnet restore --disable-parallel -p:BuildInParallel=false` |
| پورت اشغال است | برنامه قبلی را ببندید یا `--urls` بدهید |
| جدول پیدا نشد | Build و Migration را اجرا کنید؛ فایل DB ساخته‌شده با EnsureCreated پروژه دیگری را استفاده نکنید |
| UI داده ندارد | بار اول DB خالی است؛ مدرس/دانشجو اضافه کنید یا seed-demo.py را اجرا کنید |
| Fetch خطا می‌دهد | پنل اصلی را از خود Kestrel روی 5083 باز کنید، نه Live Server |
| حذف فرد 409 است | سابقه دارد؛ غیرفعال کنید یا فقط برای داده آزمایشی سوابق وابسته را حذف کنید |
| درصد حضور صفر است | Pending حاضر محسوب نمی‌شود؛ در جزئیات جلسه حضور را ثبت کنید |
| Pages ظاهر دارد ولی API ندارد | این رفتار عمدی دمو است؛ بک‌اند را محلی اجرا کنید |

## فونت

دو وزن Regular و Bold فونت Rokh از `woff2.zip` ارسالی شما استفاده شده‌اند. بسته ارسالی فایل مجوز فونت نداشت؛ مجوز تازه‌ای برای فونت تعریف نشده است. سایر وزن‌ها و فایل‌های macOS برای سبک ماندن پروژه کپی نشده‌اند.
