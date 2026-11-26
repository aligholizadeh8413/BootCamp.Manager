# مرجع API

Base URL محلی: `http://localhost:5083/api`. نام propertyها camelCase است؛ ورودی JSON با `Content-Type: application/json` ارسال شود. همه داده‌ها در SQLite ذخیره می‌شوند. Auth وجود ندارد.

## تمام Endpointها

| Method | مسیر پس از /api | بدنه | موفقیت |
|---|---|---|---|
| GET | `/students` | — | 200، آرایه StudentResponse |
| GET | `/students/{id}` | — | 200، StudentResponse |
| POST | `/students` | StudentRequest | 201 + Location + StudentResponse |
| PUT | `/students/{id}` | StudentRequest کامل | 204 |
| DELETE | `/students/{id}` | — | 204 یا 409 اگر سابقه دارد |
| GET | `/instructors` | — | 200، آرایه InstructorResponse |
| GET | `/instructors/{id}` | — | 200، InstructorResponse |
| POST | `/instructors` | InstructorRequest | 201 + Location + InstructorResponse |
| PUT | `/instructors/{id}` | InstructorRequest کامل | 204 |
| DELETE | `/instructors/{id}` | — | 204 یا 409 اگر سابقه دارد |
| GET | `/sessions` | — | 200، آرایه SessionResponse |
| GET | `/sessions/{id}` | — | 200، SessionResponse همراه شرکت‌کنندگان |
| POST | `/sessions` | CreateSessionRequest | 201 + Location + SessionResponse |
| PUT | `/sessions/{id}` | SessionRequest کامل، بدون studentIds | 204 |
| DELETE | `/sessions/{id}` | — | 204؛ حذف وابستگی‌های جلسه |
| GET | `/sessions/{id}/attendance` | — | 200، آرایه ParticipantResponse |
| PUT | `/sessions/{id}/attendance` | AttendanceRequest، کل فهرست | 204 |
| GET | `/evaluations` | — | 200، آرایه EvaluationResponse |
| GET | `/evaluations/{id}` | — | 200، EvaluationResponse |
| POST | `/evaluations` | EvaluationRequest | 201 + Location + EvaluationResponse |
| PUT | `/evaluations/{id}` | EvaluationRequest کامل | 204 |
| DELETE | `/evaluations/{id}` | — | 204 |

GETهای تکی و PUT/DELETE برای منبع ناموجود 404 می‌دهند. فهرست خالی `[]` است، نه 404. Pagination و فیلتر جلسات در مرورگر انجام می‌شود؛ query string مخصوص pagination تعریف نشده است.

## دانشجو و مدرس

```json
{"fullName":"علی رضایی","email":"ali@example.com","isActive":true}
```

StudentResponse همین فیلدها را همراه `id` دارد. InstructorRequest/Response علاوه بر آن‌ها `expertise` دارند:

```json
{"fullName":"سارا احمدی","email":"sara@example.com","expertise":"ASP.NET Core","isActive":true}
```

نام ۱ تا ۱۰۰ کاراکتر، Email معتبر تا ۲۰۰، تخصص مدرس تا ۱۵۰ و اجباری است. فاصله اطراف ایمیل حذف و ایمیل lowercase می‌شود. Email در هر جدول جداگانه unique است؛ یک ایمیل می‌تواند هم برای دانشجو و هم مدرس استفاده شود. غیرفعال شدن به معنی حذف نیست.

## ساخت جلسه

```json
{
  "title":"EF Core و Web API",
  "date":"2026-11-26",
  "instructorId":1,
  "taskDescription":"برای جلسه بعد یک CRUD کامل بسازید.",
  "progressPercent":45,
  "isCompleted":true,
  "studentIds":[1,2]
}
```

شناسه‌ها را از پاسخ POST یا GET واقعی بگیرید؛ ۱ و ۲ صرفاً نمونه‌اند. Title حداکثر ۱۵۰، TaskDescription حداکثر ۲۰۰۰ (رشته خالی مجاز)، Progress عدد صحیح ۰ تا ۱۰۰ است. تاریخ ISO فقط تاریخ است، timezone ندارد. `IsCompleted` دستی است؛ برنامه از تاریخ گذشته نتیجه نمی‌گیرد که جلسه حتماً برگزار شده است.

`studentIds` می‌تواند خالی باشد. تکرار شناسه، فرد ناموجود یا غیرفعال رد می‌شود. تمام اعضا ابتدا Pending هستند. برای ویرایش جلسه همین JSON را **بدون studentIds** بفرستید. اعضا در این نسخه عمداً بعد از ساخت تغییر نمی‌کنند؛ اگر فهرست را اشتباه ساخته‌اید، پیش از ثبت سوابق جلسه را حذف و دوباره ایجاد کنید.

SessionResponse نمونه:

```json
{
  "id":1,"title":"EF Core و Web API","date":"2026-11-26",
  "instructorId":1,"instructorName":"سارا احمدی",
  "taskDescription":"برای جلسه بعد یک CRUD کامل بسازید.",
  "progressPercent":45,"isCompleted":true,
  "participantCount":2,"presentCount":1,"pendingCount":0,"attendancePercent":50,
  "participants":[
    {"studentId":1,"fullName":"علی رضایی","status":"Present"},
    {"studentId":2,"fullName":"مریم محمدی","status":"Absent"}
  ]
}
```

## حضور و غیاب

```http
PUT /api/sessions/1/attendance
Content-Type: application/json

{"items":[{"studentId":1,"status":"Present"},{"studentId":2,"status":"Absent"}]}
```

مقادیر مجاز: `Pending` ثبت‌نشده، `Present` حاضر، `Absent` غایب. عدد enum پذیرفته نمی‌شود. هر PUT کل وضعیت جلسه را جایگزین می‌کند؛ برای هر شرکت‌کننده دقیقاً یک ورودی لازم است. شناسه اضافی، تکراری، null، وضعیت نامعتبر یا فهرست ناقص 400 می‌دهد و هیچ تغییر جزئی ذخیره نمی‌شود. برای جلسه خالی `{"items":[]}` معتبر است.

حضور = Present / کل فهرست × ۱۰۰ با گرد کردن به یک رقم اعشار. Pending در مخرج هست اما در صورت نیست. پس اگر ۱ حاضر، ۱ غایب و ۱ ثبت‌نشده داشته باشیم، حضور ۳۳٫۳٪ و PendingCount برابر ۱ است. کارت کلی صفحه میانگین وزنی بر اساس تعداد شرکت‌کنندگان جلسات **برگزارشده** دارد؛ میانگین ساده درصدها نیست.

## ارزیابی

```json
{"sessionId":1,"studentId":1,"instructorId":1,"score":88,"notes":"CRUD درست است؛ اعتبارسنجی را کامل‌تر کن."}
```

Score عدد صحیح ۰ تا ۱۰۰ و Notes حداکثر ۲۰۰۰ (خالی مجاز) است. دانشجو باید عضو جلسه باشد. مدرس ارزیاب باید موجود باشد؛ الزام ندارد مدرس همان جلسه یا فعال باشد تا امکان اصلاح سوابق باقی بماند. هر زوج Session/Student تنها یک ارزیابی دارد؛ تکرار 409 است و برای تغییر باید PUT بفرستید.

خروجی افزون بر بدنه ورودی، `id`, `sessionTitle`, `studentName`, `instructorName` دارد. PUT می‌تواند مرجع جلسه/دانشجو/مدرس را نیز تغییر دهد، به شرط معتبر بودن عضویت و عدم تکرار.

## کدهای خطا

| کد | مثال |
|---|---|
| 400 | DTO نامعتبر، ایمیل بدفرمت، progress=101، roster ناقص، مدرس ناموجود |
| 404 | منبعی با Id خواسته‌شده پیدا نشد |
| 409 | ایمیل یا ارزیابی تکراری، حذف فرد دارای سابقه، تعارض FK/Unique |
| 500 | خطای غیرمنتظره؛ پیام عمومی و traceId، جزئیات فقط در لاگ سرور |

پیام‌های DTO و قواعد کسب‌وکار فارسی‌اند. خطاهای تبدیل JSON نیز در Program.cs به پیام عمومی فارسی تبدیل می‌شوند. نام کلیدها در JSON همان قرارداد camelCase است.

```json
{"status":409,"title":"این ایمیل قبلاً ثبت شده است.","instance":"/api/students","traceId":"..."}
```

خطای DataAnnotations معمولاً `errors` هم دارد. frontend ابتدا آرایه errors و سپس title را نمایش می‌دهد. مقدار `traceId` برای تطبیق با لاگ درخواست است.

## HTTP Client

فایل ریشه `BootCamp.Manager.http` را در VS Code همراه افزونه REST Client باز کنید؛ ترتیب درخواست‌ها از بالا به پایین است. syntax دریافت ID از پاسخ به REST Client مربوط است. برای ابزار دیگر، IDها را دستی جایگزین کنید. آخر فایل cleanup دارد؛ فقط ردیف‌های ساخته‌شده در همان توالی را حذف می‌کند.
