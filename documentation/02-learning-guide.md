# آموزش از Console و FastAPI تا ASP.NET Core

## ۱. جابه‌جایی چهار عملیات آشنا

در Console از `Console.ReadLine()` ورودی می‌گرفتید. اکنون مرورگر با Fetch یک JSON می‌فرستد. هر دو در نهایت به همان `Add`، Query، تغییر property و `Remove` می‌رسند.

| عملیات قبلی | مسیر پروژه جدید |
|---|---|
| `AddBook(db)` | POST → Controller → Service → Repository.Add → SaveChangesAsync |
| `ShowBooks(db)` | GET → Repository.GetAllAsync → ToListAsync |
| `EditBook(db)` | PUT → GetByIdAsync → تغییر property → SaveChangesAsync |
| `DeleteBook(db)` | DELETE → بررسی وجود/روابط → Remove → SaveChangesAsync |

برای شروع فقط مسیر دانشجو را بخوانید. جلسه و ارزیابی روی همین الگو، قوانین رابطه‌ها را اضافه می‌کنند.

## ۲. Entity، DbSet و DbContext

`Domain/Entities/Student.cs` یک کلاس C# معمولی است. `Id` کلید، `FullName` و `Email` مشخصات، `IsActive` وضعیت است. این کلاس HTTP response نیست و از EF ارث نمی‌برد.

```csharp
public DbSet<Student> Students => Set<Student>();
```

DbSet نماینده مجموعه Entityها برای Query و تغییرات است؛ تمام جدول را در RAM بارگذاری نمی‌کند. کد `db.Students.Where(...)` یک Query می‌سازد و `ToListAsync` آن را اجرا می‌کند.

`BootcampDbContext` اتصال، مدل جدول‌ها، Change Tracking و ذخیره تغییرات را هماهنگ می‌کند. `OnModelCreating` برای Email ایندکس Unique، برای رابطه‌ها Foreign Key و برای Score و Progress قید CHECK تعریف می‌کند. در SQLite طول متن با `HasMaxLength` به‌تنهایی محدود نمی‌شود؛ بنابراین DTO هم `[StringLength]` دارد.

```csharp
var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
student!.FullName = "نام جدید";
await db.SaveChangesAsync(ct);
```

Query تکی ما Tracking دارد؛ EF مقدار قبلی و جدید را مقایسه و UPDATE می‌سازد. Query فهرست با `AsNoTracking` فقط برای خواندن است و هزینه Tracking ندارد. `Add` و `Remove` به‌تنهایی SQL نهایی را اجرا نمی‌کنند؛ `SaveChangesAsync` لازم است.

DbContext باید Scoped باشد. آن را static یا singleton نکنید؛ Thread-safe نیست. DI در پایان درخواست dispose را انجام می‌دهد. `async/await` اجازه می‌دهد زمان انتظار دیتابیس، thread درخواست بی‌جهت اشغال نماند؛ خودبه‌خود عملیات را موازی نمی‌کند. CancellationToken لغو درخواست را به EF می‌رساند.

## ۳. Migration به جای EnsureCreated

در پروژه قبلی `EnsureCreated()` برای ساخت اولیه کافی بود. در این پروژه فقط Migration داریم و این دو روش را ترکیب نمی‌کنیم.

| فایل | مسئولیت |
|---|---|
| `..._InitialCreate.cs` | `Up` ساخت جدول‌ها/ایندکس‌ها؛ `Down` بازگردانی همان تغییر |
| `..._InitialCreate.Designer.cs` | متادیتای مدل همان Migration؛ تولید EF |
| `BootcampDbContextModelSnapshot.cs` | آخرین مدل برای مقایسه تغییر بعدی؛ تولید EF |

`migrations add` تغییر مدل را به فایل کد تبدیل می‌کند؛ `database update` آن را روی دیتابیس اعمال می‌کند. `__EFMigrationsHistory` نام Migrationهای اعمال‌شده را نگه می‌دارد. فایل DB داخل ZIP نیست؛ Migration امکان ساخت تکرارپذیر آن را فراهم می‌کند.

## ۴. Repository و Service

`IStudentRepository` در Application قرارداد عملیات ذخیره‌سازی را تعریف می‌کند. `StudentRepository` در Infrastructure می‌گوید این عملیات چگونه با EF اجرا شود. Controller لازم نیست بداند SQLite چیست.

`StudentService` درباره ایمیل تکراری و ممنوعیت حذف فرد دارای سابقه تصمیم می‌گیرد. `SessionService` فهرست مستقل جلسه، مدرس معتبر و حضور را مدیریت می‌کند. `EvaluationService` عضویت دانشجو و یکتا بودن ارزیابی را بررسی می‌کند. این‌ها قوانین پروژه‌اند و در UI دوباره منبع اصلی حقیقت نمی‌شوند.

برای ساده ماندن، بررسی ایمیل تکراری از فهرست دانشجویان استفاده می‌کند. این برای داده کم آموزشی قابل فهم است؛ در پروژه بزرگ می‌توان `EmailExistsAsync` و pagination دیتابیسی نوشت. Unique index دیتابیس حتی در درخواست‌های هم‌زمان هم محافظ نهایی است.

چهار Repository عمداً صریح هستند؛ Generic Repository یا UnitOfWork سفارشی نداریم. DbContext موجود عملیات یک درخواست را ذخیره می‌کند. متد GetById ردیف Tracking‌شده را می‌دهد و GetAll برای نمایش است.

## ۵. DTO، Record و اعتبارسنجی

Entity مدل ذخیره‌سازی است؛ DTO قرارداد ورودی/خروجی API است. `StudentRequest` اجازه نمی‌دهد کاربر `Id` دیتابیس را تعیین کند. `StudentResponse` فقط فیلدهای لازم را برمی‌گرداند. Navigation propertyهای EF به JSON فرستاده نمی‌شوند؛ بنابراین حلقه Student → Session → Student نداریم.

در `Requests.cs` از کلاس با property استفاده شده تا فرم و اعتبارسنجی خوانا باشد. `Responses.cs` از `record` برای داده خروجی استفاده می‌کند، مشابه DTOهای اسلایدها.

```csharp
[Required, EmailAddress, StringLength(200)]
public string Email { get; set; } = "";
```

`[ApiController]` با ورودی نامعتبر معمولاً پیش از Action پاسخ 400 می‌دهد. Required نام خالی و whitespace را رد می‌کند. `Range(0,100)` برای امتیاز و پیشرفت است. تبدیل enum فقط رشته‌های Pending/Present/Absent را قبول می‌کند؛ عدد enum در JSON رد می‌شود.

اعتبارسنجی سه بخش دارد: DTO برای شکل داده، Service برای روابط و قواعد، DB برای قیدهای نهایی. خطای Unique یا FK احتمالی در Middleware به 409 تبدیل می‌شود. خطای پیش‌بینی‌نشده با پیام عمومی 500 پاسخ می‌گیرد و stack trace فقط در لاگ سرور است.

## ۶. Dependency Injection در Program.cs

```csharp
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<StudentService>();
```

وقتی ASP.NET می‌خواهد `StudentsController(StudentService service)` را بسازد، Container ابتدا Service و سپس Repository و DbContext مورد نیاز آن را فراهم می‌کند. Scoped یعنی در محدوده یک درخواست نمونه مشترک داریم؛ هر درخواست معمولاً نمونه خودش را می‌گیرد.

`Program.cs` محل اتصال لایه‌هاست. Application فقط interface را می‌شناسد؛ WebApi می‌گوید پیاده‌سازی واقعی آن کدام است. برای Service چهار interface اضافی نساخته‌ایم، چون در این تمرین فقط یک پیاده‌سازی از هر Service وجود دارد.

## ۷. Controller و Routing

```csharp
[ApiController]
[Route("api/students")]
public class StudentsController(StudentService service) : ControllerBase
```

`[HttpGet("{id:int}")]` یعنی GET با یک بخش عدد صحیح در مسیر. مقدار مسیر به `int id` متد bind می‌شود. `POST` و `PUT` در این API بدنه JSON دارند. `CancellationToken` از وضعیت درخواست گرفته می‌شود.

Controller مسئول دریافت ورودی، فراخوانی Service و انتخاب پاسخ HTTP است: `Ok` برابر 200، `CreatedAtAction` برابر 201 همراه Location، `NoContent` برابر 204. اگر POST شناسه 7 بسازد، Location به `/api/students/7` اشاره می‌کند.

هر PUT باید تمام فیلدهای قابل ویرایش آن DTO را بفرستد؛ PATCH نیست. Id در URL است، نه در DTO. فیلدهای bool/عدد در نبود ورودی مقدار پیش‌فرض دارند؛ فرانت‌اند تمام فیلدها را ارسال می‌کند. PUT جلسه شرکت‌کنندگان را تغییر نمی‌دهد؛ قرارداد CreateSessionRequest برای ایجاد، `studentIds` اضافی دارد.

## ۸. فرانت‌اند، Fetch و دو نوع داده

`wwwroot/index.html` پوسته، Navbar، محل کارت‌ها و dialog مشترک را تعریف می‌کند. `styles.css` رنگ، فونت، grid، responsive و حالت Modal را می‌سازد. `app.js` state ساده، صفحه‌بندی، انتخاب تب، فرم‌ها و نمایش خطا را کنترل می‌کند. `data.js` تنها لایه ارتباطی آن با HTTP است.

```javascript
await fetch('/api/students', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ fullName: 'علی', email: 'ali@example.com', isActive: true })
});
```

چون wwwroot و API از همان host/port سرو می‌شوند، CORS لازم نیست. 204 بدنه ندارد؛ data.js تلاش نمی‌کند آن را JSON کند. خطاهای ValidationProblemDetails و ProblemDetails داخل فرم نشان داده می‌شوند. متن کاربر پیش از قرار گرفتن در HTML escape می‌شود.

فرانت‌اند اصلی هیچ داده ساختگی و fallback پنهانی ندارد. در `docs/js/data.js` همان interface جاوااسکریپتی `api` با آرایه‌های ساختگی و localStorage پیاده‌سازی شده است. قواعد اصلی دمو بازسازی شده‌اند، ولی دمو جای آزمون بک‌اند نیست. `scripts/sync-demo.py` فایل‌های مشترک را کپی می‌کند و adapter دمو را حفظ می‌کند.

کارت‌ها روی دسکتاپ عریض ۶ عدد، روی عرض متوسط ۴ عدد و روی موبایل ۶ عدد در هر صفحه‌اند. API فعلاً تمام ردیف‌ها را می‌فرستد و pagination در مرورگر است. در ارتفاع بسیار کم (زیر 590px)، اسکرول برای خوانایی مجاز می‌شود؛ در موبایل هم اسکرول طبیعی داریم. هدف صفحه اصلی دسکتاپ معمولی بدون اسکرول است؛ این رفتار هنوز به‌صورت تصویری در محیط تحویل تأیید نشده است.

## ۹. مقایسه با چیزهایی که می‌شناسید

| ASP.NET Core | نزدیک‌ترین مفهوم در FastAPI/Python | تفاوت مهم |
|---|---|---|
| Controller + `[HttpGet]` | APIRouter + `@router.get` | مسیرها روی کلاس و متد تعریف می‌شوند |
| Request DTO + DataAnnotations | Pydantic model | اعتبارسنجی ورودی با Attribute و ModelState |
| Response record | response_model | قرارداد خروجی مستقل از ORM |
| AddScoped / Constructor Injection | Depends | طول عمر وابستگی‌ها در container تنظیم می‌شود |
| DbContext | SQLAlchemy Session | Tracking و SaveChanges مشابه unit of work |
| DbSet | Query/select برای یک مدل | مجموعه قابل Query است؛ لیست حافظه‌ای نیست |
| EF Migration | Alembic migration | نسخه‌بندی schema؛ SQL تابع Provider است |
| Middleware | `@app.middleware('http')` | قبل/بعد بخش بعدی pipeline |
| IActionResult | Response / JSONResponse | انتخاب status، header و body |
| Controller با View | route + Jinja template | Razor در سرور HTML می‌سازد |

این جدول برای پل ذهنی است؛ APIها دقیقاً معادل یکدیگر نیستند.

## ۱۰. SQL قابل توضیح در کلاس

```sql
-- شرکت‌کنندگان یک جلسه، مستقل از تعداد کل دانشجویان
SELECT p.StudentId, s.FullName, p.Status
FROM SessionParticipants AS p
JOIN Students AS s ON s.Id = p.StudentId
WHERE p.SessionId = 1;

-- همان فرمول حضور؛ NULL وقتی جلسه شرکت‌کننده ندارد
SELECT se.Id, COUNT(p.StudentId) AS Total,
       ROUND(100.0 * SUM(CASE WHEN p.Status = 'Present' THEN 1 ELSE 0 END)
             / NULLIF(COUNT(p.StudentId), 0), 1) AS AttendancePercent
FROM Sessions AS se
LEFT JOIN SessionParticipants AS p ON p.SessionId = se.Id
GROUP BY se.Id;
```

فرمول در کد فعلی Service محاسبه می‌شود؛ Query بالا معادل آموزشی SQL آن است. درصد حضور را به‌عنوان ستون جدا ذخیره نکرده‌ایم تا دو مقدار ناسازگار نداشته باشیم. پیشرفت درس یک عدد مستقل است که مدرس وارد می‌کند؛ از حضور یا امتیاز استنتاج نمی‌شود.
