// این فایل فقط در docs استفاده می‌شود؛ بک‌اند یا SQLite ندارد.
export const isDemo = true;
const key = 'bootcamp-manager-demo-v1';
function initialData() {
    const names = ['علی رضایی', 'سارا احمدی', 'محمد کریمی', 'نگار محمدی', 'رضا حسینی', 'مریم عباسی', 'امیر نادری', 'نازنین صادقی', 'حسین موسوی', 'زهرا مرادی', 'پویا اکبری', 'هانیه کاظمی'];
    const students = names.map((fullName, i) => ({ id: i + 1, fullName, email: `student${i + 1}@example.com`, isActive: true }));
    const instructors = [{ id: 1, fullName: 'آرش محمدی', email: 'arash@example.com', expertise: 'C# و ASP.NET Core', isActive: true }, { id: 2, fullName: 'مهسا کریمی', email: 'mahsa@example.com', expertise: 'دیتابیس و معماری نرم‌افزار', isActive: true }];
    const titles = ['مبانی C# و شیءگرایی', 'آشنایی با Entity Framework', 'Web API و EF Core', 'معماری Web API', 'Middleware و Action Filter', 'معماری لایه‌ای و سرویس‌ها'];
    const tasks = ['تمرین کلاس و آبجکت', 'مدیریت کتاب‌ها', 'ساخت پروژه CRUD', 'ساخت Service Layer', 'ثبت لاگ درخواست', 'تکمیل پنل بوت‌کمپ'];
    const sessions = titles.map((title, i) => ({ id: i + 1, title, date: `2026-11-${String(14 + i * 2).padStart(2, '0')}`, instructorId: i > 3 ? 2 : 1, taskDescription: tasks[i], progressPercent: [15, 30, 45, 65, 80, 0][i], isCompleted: i < 5, participants: students.map((s, j) => ({ studentId: s.id, status: i === 5 ? 'Pending' : (j < [11, 10, 12, 10, 11][i] ? 'Present' : 'Absent') })) }));
    return { students, instructors, sessions, evaluations: [{ id: 1, sessionId: 3, studentId: 1, instructorId: 1, score: 88, notes: 'پیاده‌سازی CRUD درست است؛ اعتبارسنجی ورودی کامل‌تر شود.' }] };
}
let db;
try { db = JSON.parse(localStorage.getItem(key)) || initialData(); } catch { db = initialData(); }
function save() { try { localStorage.setItem(key, JSON.stringify(db)); } catch { throw new Error('مرورگر امکان ذخیره داده نمایشی را نمی‌دهد.'); } }
function fail(message) { throw new Error(message); }
function find(type, id) { return db[type].find(x => x.id === id) || fail('مورد پیدا نشد.'); }
function requireText(value, length, label) { if (typeof value !== 'string' || !value.trim() || value.length > length) fail(`${label} معتبر وارد کنید.`); }
function validate(type, data, id = 0) {
    if (type === 'students' || type === 'instructors') {
        requireText(data.fullName, 100, 'نام'); requireText(data.email, 200, 'ایمیل');
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(data.email)) fail('ایمیل نامعتبر است.');
        if (db[type].some(x => x.id !== id && x.email.toLowerCase() === data.email.trim().toLowerCase())) fail('این ایمیل قبلاً ثبت شده است.');
        if (type === 'instructors') requireText(data.expertise, 150, 'تخصص');
    }
    if (type === 'sessions') {
        requireText(data.title, 150, 'عنوان');
        if (!data.date || Number.isNaN(Date.parse(data.date))) fail('تاریخ معتبر وارد کنید.');
        const instructor = find('instructors', data.instructorId);
        if (!instructor.isActive && (!id || find(type, id).instructorId !== instructor.id)) fail('مدرس فعال انتخاب کنید.');
        if (!Number.isInteger(data.progressPercent) || data.progressPercent < 0 || data.progressPercent > 100) fail('پیشرفت باید بین صفر تا صد باشد.');
        if ((data.taskDescription || '').length > 2000) fail('متن تسک بیش از حد طولانی است.');
        if (!id && (new Set(data.studentIds).size !== data.studentIds.length || data.studentIds.some(s => !find('students', s).isActive))) fail('فهرست دانشجویان نامعتبر است.');
    }
    if (type === 'evaluations') {
        if (!find('sessions', data.sessionId).participants.some(p => p.studentId === data.studentId)) fail('دانشجو باید شرکت‌کننده این جلسه باشد.');
        find('instructors', data.instructorId);
        if (!Number.isInteger(data.score) || data.score < 0 || data.score > 100) fail('امتیاز باید بین صفر تا صد باشد.');
        if ((data.notes || '').length > 2000) fail('توضیحات بیش از حد طولانی است.');
        if (db.evaluations.some(e => e.id !== id && e.studentId === data.studentId && e.sessionId === data.sessionId)) fail('ارزیابی تکراری است؛ مورد قبلی را ویرایش کنید.');
    }
}
function response(type, row) {
    if (type === 'sessions') {
        const participants = row.participants.map(p => ({ ...p, fullName: find('students', p.studentId).fullName }));
        const presentCount = participants.filter(p => p.status === 'Present').length;
        return { ...row, participants, instructorName: find('instructors', row.instructorId).fullName, participantCount: participants.length, presentCount, pendingCount: participants.filter(p => p.status === 'Pending').length, attendancePercent: participants.length ? Math.round(presentCount * 1000 / participants.length) / 10 : null };
    }
    if (type === 'evaluations') return { ...row, studentName: find('students', row.studentId).fullName, sessionTitle: find('sessions', row.sessionId).title, instructorName: find('instructors', row.instructorId).fullName };
    return { ...row };
}
// هر تغییر ابتدا روی کپی اجرا می‌شود؛ خطای ذخیره، داده در حافظه را هم برمی‌گرداند.
async function mutate(action) {
    const previous = structuredClone(db);
    try { const result = action(); save(); return result; }
    catch (error) { db = previous; throw error; }
}
export const resetDemo = () => mutate(() => { db = initialData(); });
export const api = {
    list: async type => db[type].map(row => response(type, row)),
    create: (type, data) => mutate(() => {
        validate(type, data);
        const row = { ...data, id: Math.max(0, ...db[type].map(x => x.id)) + 1 };
        if (type === 'sessions') { row.participants = data.studentIds.map(studentId => ({ studentId, status: 'Pending' })); delete row.studentIds; }
        if (row.email) row.email = row.email.trim().toLowerCase();
        db[type].push(row); return response(type, row);
    }),
    update: (type, id, data) => mutate(() => {
        const row = find(type, id); validate(type, data, id);
        Object.assign(row, data); if (row.email) row.email = row.email.trim().toLowerCase();
    }),
    remove: (type, id) => mutate(() => {
        find(type, id);
        if (type === 'students' && db.sessions.some(s => s.participants.some(p => p.studentId === id))) fail('این دانشجو سابقه دارد؛ او را غیرفعال کنید.');
        if (type === 'instructors' && (db.sessions.some(s => s.instructorId === id) || db.evaluations.some(e => e.instructorId === id))) fail('این مدرس سابقه دارد؛ او را غیرفعال کنید.');
        db[type] = db[type].filter(x => x.id !== id);
        if (type === 'sessions') db.evaluations = db.evaluations.filter(e => e.sessionId !== id);
    }),
    attendance: (id, items) => mutate(() => {
        const session = find('sessions', id);
        if (items.length !== session.participants.length || new Set(items.map(i => i.studentId)).size !== items.length || items.some(i => !['Pending', 'Present', 'Absent'].includes(i.status) || !session.participants.some(p => p.studentId === i.studentId))) fail('حضور و غیاب نامعتبر است.');
        session.participants = items.map(i => ({ ...i }));
    })
};
