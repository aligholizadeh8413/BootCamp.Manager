import { api, isDemo, resetDemo } from './data.js';
const $ = selector => document.querySelector(selector);
const state = { tab: 'sessions', filter: 'all', page: 1, students: [], instructors: [], sessions: [], evaluations: [] };
const fa = value => new Intl.NumberFormat('fa-IR', { maximumFractionDigits: 1 }).format(value);
const date = value => new Intl.DateTimeFormat('fa-IR', { year: 'numeric', month: 'long', day: 'numeric' }).format(new Date(`${value}T12:00:00`));
// هر متن واردشده توسط کاربر، پیش از قرار گرفتن در HTML، escape می‌شود.
const escape = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
const titles = { sessions: ['جلسه‌های بوت‌کمپ', 'جلسه جدید', 'همه جلسه‌ها'], students: ['دانشجویان بوت‌کمپ', 'دانشجوی جدید', 'فهرست دانشجویان'], instructors: ['مدرسان بوت‌کمپ', 'مدرس جدید', 'فهرست مدرسان'], evaluations: ['ارزیابی عملکرد', 'ارزیابی جدید', 'ارزیابی دانشجویان'] };
const icons = {
    users: '<circle cx="9" cy="7" r="3"/><path d="M3 21v-4a6 6 0 0 1 12 0v4M16 4a3 3 0 0 1 0 6m2 3a5 5 0 0 1 3 4v4"/>',
    calendar: '<rect x="4" y="5" width="16" height="16" rx="3"/><path d="M8 3v5m8-5v5M4 11h16m-12 4h3m3 0h2"/>',
    chart: '<path d="M4 20h16M5 15l5-5 4 3 6-8m-5 0h5v5"/>'
};
function size() { return state.tab === 'sessions' ? (innerWidth > 700 && innerWidth <= 1100 ? 4 : 6) : Math.max(3, Math.min(7, Math.floor((innerHeight - 390) / 59))); }
function percent(n) { return n === null ? '—' : `${fa(n)}٪`; }
function toast(message) { $('#toast').textContent = message; $('#toast').hidden = false; setTimeout(() => $('#toast').hidden = true, 3200); }
async function refresh() {
    $('#content').setAttribute('aria-busy', 'true');
    $('#page-error').hidden = true;
    try {
        const values = await Promise.all(['students', 'instructors', 'sessions', 'evaluations'].map(type => api.list(type)));
        ['students', 'instructors', 'sessions', 'evaluations'].forEach((type, i) => state[type] = values[i]);
        render();
    } catch (error) {
        $('#page-error').textContent = error.message; $('#page-error').hidden = false;
        $('#content').innerHTML = '<div class="empty"><h2>داده‌ها دریافت نشدند</h2><button data-retry>تلاش دوباره</button></div>';
    } finally { $('#content').setAttribute('aria-busy', 'false'); }
}
function render() {
    const [title, create, list] = titles[state.tab];
    $('#page-title').textContent = title; $('#create-label').textContent = create; $('#list-title').textContent = list;
    $('#subtitle').textContent = state.tab === 'sessions' ? 'مدیریت جلسات، حضور و غیاب و پیشرفت دوره در یک نگاه' : 'اطلاعات دوره را مشاهده، ثبت و ویرایش کنید';
    document.querySelectorAll('[data-tab]').forEach(b => { const active = b.dataset.tab === state.tab; b.classList.toggle('active', active); active ? b.setAttribute('aria-current', 'page') : b.removeAttribute('aria-current'); });
    $('#filters').hidden = state.tab !== 'sessions';
    document.querySelectorAll('[data-filter]').forEach(b => { const active = b.dataset.filter === state.filter; b.classList.toggle('active', active); b.setAttribute('aria-pressed', active); });
    const completed = state.sessions.filter(s => s.isCompleted);
    const totalParticipants = completed.reduce((sum, s) => sum + s.participantCount, 0);
    const attendance = totalParticipants ? completed.reduce((sum, s) => sum + s.presentCount, 0) * 100 / totalParticipants : null;
    $('#stats').innerHTML = [['تعداد دانشجویان', fa(state.students.length), 'users'], ['جلسات برگزارشده', `${fa(completed.length)} <small class="muted">از ${fa(state.sessions.length)} جلسه</small>`, 'calendar'], ['حضور کل جلسات برگزارشده', percent(attendance), 'chart']].map(([label, value, icon]) => `<div class="stat"><div><p class="muted">${label}</p><strong>${value}</strong></div><span class="stat-icon"><svg viewBox="0 0 24 24" aria-hidden="true">${icons[icon]}</svg></span></div>`).join('');
    let rows = [...state[state.tab]];
    if (state.tab === 'sessions') rows = rows.filter(s => state.filter === 'all' || (state.filter === 'completed' ? s.isCompleted : !s.isCompleted)).sort((a, b) => b.date.localeCompare(a.date) || b.id - a.id);
    else rows.sort((a, b) => b.id - a.id);
    const pageSize = size(), pages = Math.max(1, Math.ceil(rows.length / pageSize));
    state.page = Math.max(1, Math.min(state.page, pages));
    const slice = rows.slice((state.page - 1) * pageSize, state.page * pageSize);
    $('#total').textContent = `${fa(rows.length)} مورد`;
    $('#previous').disabled = state.page === 1; $('#next').disabled = state.page === pages;
    $('#page-number').textContent = fa(state.page);
    $('#page-description').textContent = rows.length ? `نمایش ${fa((state.page - 1) * pageSize + 1)} تا ${fa(Math.min(state.page * pageSize, rows.length))} از ${fa(rows.length)} مورد` : 'هنوز موردی ثبت نشده است';
    if (!rows.length) {
        $('#content').innerHTML = `<div class="empty"><h2>اینجا هنوز خالی است</h2><p>${state.tab === 'sessions' ? 'ابتدا مدرس و دانشجویان را ثبت کنید، سپس جلسه بسازید.' : 'اولین مورد را با دکمه بالای صفحه ثبت کنید.'}</p></div>`; return;
    }
    $('#content').innerHTML = state.tab === 'sessions' ? `<div class="cards">${slice.map(card).join('')}</div>` : table(slice);
}
function metric(label, value, color) { return `<div><div class="metric-label"><span>${label}</span><b>${percent(value)}</b></div><div class="track ${color}"><i style="width:${value ?? 0}%"></i></div></div>`; }
function card(s) { return `<button class="session-card" data-session="${s.id}" aria-label="جزئیات ${escape(s.title)}"><span class="card-top"><span class="card-id"><span class="number-badge">${fa(s.id)}</span>جلسه ${fa(s.id)}</span><span class="badge ${s.isCompleted ? '' : 'future'}">${s.isCompleted ? 'برگزارشده' : 'برنامه‌ریزی‌شده'}</span></span><h3>${escape(s.title)}</h3><span class="date">${date(s.date)}</span><div class="metrics">${metric('حضور دانشجویان', s.attendancePercent, '')}${metric('پیشرفت درس', s.progressPercent, 'orange')}</div><span class="card-footer"><span class="task-text" title="${escape(s.taskDescription)}">تسک: ${escape(s.taskDescription || 'تعیین نشده')}</span><span class="detail-link">جزئیات جلسه ←</span></span></button>`; }
function table(rows) {
    const evaluation = state.tab === 'evaluations';
    const headers = evaluation ? ['دانشجو', 'جلسه', 'امتیاز', 'مدرس', 'عملیات'] : ['نام و نام خانوادگی', 'ایمیل', state.tab === 'instructors' ? 'تخصص' : 'وضعیت', 'عملیات'];
    return `<div class="table-wrap"><table><thead><tr>${headers.map(h => `<th scope="col">${h}</th>`).join('')}</tr></thead><tbody>${rows.map(row => `<tr>${evaluation ? `<td class="table-name">${escape(row.studentName)}</td><td class="truncate">${escape(row.sessionTitle)}</td><td>${fa(row.score)} از ۱۰۰</td><td>${escape(row.instructorName)}</td>` : `<td class="table-name">${escape(row.fullName)}${!row.isActive ? ' (غیرفعال)' : ''}</td><td class="ltr">${escape(row.email)}</td><td>${state.tab === 'instructors' ? escape(row.expertise) : (row.isActive ? 'فعال' : 'غیرفعال')}</td>`}<td><div class="actions"><button data-edit="${row.id}">ویرایش</button><button data-delete="${row.id}" class="danger">حذف</button></div></td></tr>`).join('')}</tbody></table></div>`;
}
function modal(title, html) { $('#modal-title').textContent = title; $('#modal-content').innerHTML = html; if (!$('#modal').open) $('#modal').showModal(); }
function field(label, name, value = '', type = 'text', attrs = '') { return `<label class="field">${label}<input name="${name}" type="${type}" value="${escape(value)}" ${attrs}></label>`; }
function options(rows, selected, label = 'fullName') { return rows.map(r => `<option value="${r.id}" ${Number(selected) === r.id ? 'selected' : ''}>${escape(r[label])}${r.isActive === false ? ' (غیرفعال)' : ''}</option>`).join(''); }
function select(label, name, rows, selected, key) { return `<label class="field">${label}<select name="${name}" required><option value="">انتخاب کنید</option>${options(rows, selected, key)}</select></label>`; }
function textarea(label, name, value) { return `<label class="field full">${label}<textarea name="${name}" maxlength="2000">${escape(value ?? '')}</textarea></label>`; }
const actions = '<p class="form-error" role="alert" hidden></p><div class="form-actions"><button type="button" data-cancel>انصراف</button><button type="submit" class="primary">ذخیره اطلاعات</button></div>';
function openEditor(type, id) {
    const row = state[type].find(r => r.id === id) || {};
    let fields = '';
    if (type === 'students' || type === 'instructors') {
        fields = field('نام و نام خانوادگی', 'fullName', row.fullName, 'text', 'required maxlength="100"') + field('ایمیل', 'email', row.email, 'email', 'required maxlength="200" dir="ltr"');
        if (type === 'instructors') fields += field('تخصص', 'expertise', row.expertise, 'text', 'required maxlength="150"');
        fields += `<label class="check"><input name="isActive" type="checkbox" ${row.isActive !== false ? 'checked' : ''}>فعال</label>`;
    } else if (type === 'sessions') {
        fields = field('عنوان جلسه', 'title', row.title, 'text', 'required maxlength="150"') + field('تاریخ جلسه (ورودی میلادی، نمایش شمسی)', 'date', row.date, 'date', 'required') + select('مدرس', 'instructorId', state.instructors.filter(i => i.isActive || i.id === row.instructorId), row.instructorId) + field('پیشرفت درس (۰ تا ۱۰۰)', 'progressPercent', row.progressPercent ?? 0, 'number', 'required min="0" max="100" step="1"') + textarea('تسک این جلسه برای جلسه بعد', 'taskDescription', row.taskDescription) + `<label class="check"><input name="isCompleted" type="checkbox" ${row.isCompleted ? 'checked' : ''}>جلسه برگزار شده است</label>`;
        fields += id ? '<p class="field full muted">فهرست شرکت‌کنندگان هنگام ایجاد جلسه ثبت شده و با ویرایش مشخصات تغییر نمی‌کند.</p>' : `<fieldset><legend>شرکت‌کنندگان این جلسه</legend><p class="muted">دانشجویان فعال از دیتابیس خوانده شده‌اند. جلسه بدون شرکت‌کننده نیز مجاز است.</p><div class="check-list">${state.students.filter(s => s.isActive).map(s => `<label class="check"><input name="studentIds" value="${s.id}" type="checkbox" checked>${escape(s.fullName)}</label>`).join('') || '<p class="muted">دانشجوی فعالی ثبت نشده است.</p>'}</div></fieldset>`;
    } else {
        fields = select('جلسه', 'sessionId', state.sessions, row.sessionId, 'title') + '<label class="field">دانشجو<select name="studentId" required></select></label>' + select('مدرس ارزیاب', 'instructorId', state.instructors, row.instructorId) + field('امتیاز (۰ تا ۱۰۰)', 'score', row.score ?? 0, 'number', 'required min="0" max="100" step="1"') + textarea('توضیحات مدرس', 'notes', row.notes);
    }
    modal(id ? 'ویرایش اطلاعات' : titles[type][1], `<form id="editor"><div class="form-grid">${fields}</div>${actions}</form>`);
    if (type === 'evaluations') {
        const sessionSelect = $('#editor [name=sessionId]');
        const updateStudents = () => {
            const session = state.sessions.find(s => s.id === Number(sessionSelect.value));
            $('#editor [name=studentId]').innerHTML = '<option value="">انتخاب کنید</option>' + options((session?.participants ?? []).map(p => ({ id: p.studentId, fullName: p.fullName })), row.studentId);
        };
        sessionSelect.addEventListener('change', updateStudents); updateStudents();
    }
    $('#editor').addEventListener('submit', event => submit(event, async form => {
        const data = Object.fromEntries(form.entries());
        for (const key of ['instructorId', 'sessionId', 'studentId', 'score', 'progressPercent']) if (key in data) data[key] = Number(data[key]);
        if (type === 'students' || type === 'instructors') data.isActive = form.has('isActive');
        if (type === 'sessions') { data.isCompleted = form.has('isCompleted'); if (!id) data.studentIds = form.getAll('studentIds').map(Number); }
        await (id ? api.update(type, id, data) : api.create(type, data));
    }));
}
async function submit(event, save) {
    event.preventDefault();
    const form = event.currentTarget, button = form.querySelector('[type=submit]'), error = form.querySelector('.form-error');
    error.hidden = true; button.disabled = true;
    try { await save(new FormData(form)); $('#modal').close(); await refresh(); toast('اطلاعات ذخیره شد'); }
    catch (e) { error.textContent = e.message; error.hidden = false; }
    finally { button.disabled = false; }
}
function openSession(id) {
    const s = state.sessions.find(x => x.id === id);
    const evaluationCount = state.evaluations.filter(e => e.sessionId === id).length;
    modal(s.title, `<div class="detail-summary"><p>تاریخ: ${date(s.date)}</p><p>مدرس: ${escape(s.instructorName)}</p><p>حضور: ${percent(s.attendancePercent)} (${fa(s.presentCount)} از ${fa(s.participantCount)})</p><p>ثبت‌نشده: ${fa(s.pendingCount)} / ارزیابی: ${fa(evaluationCount)}</p><p>پیشرفت درس: ${percent(s.progressPercent)}</p><p class="task-detail">تسک جلسه بعد: ${escape(s.taskDescription || 'تعیین نشده')}</p></div><div class="small-actions"><button id="edit-session">ویرایش جلسه</button><button id="delete-session" class="danger">حذف جلسه</button></div><h2>حضور و غیاب</h2><p class="dialog-description">درصد حضور از کل فهرست این جلسه محاسبه می‌شود. ثبت‌نشده به معنی حاضر نیست.</p><form id="attendance-form">${s.participants.map(p => `<label class="attendance-row"><span>${escape(p.fullName)}</span><select name="${p.studentId}">${[['Pending', 'ثبت‌نشده'], ['Present', 'حاضر'], ['Absent', 'غایب']].map(([value, label]) => `<option value="${value}" ${p.status === value ? 'selected' : ''}>${label}</option>`).join('')}</select></label>`).join('') || '<p class="muted">این جلسه شرکت‌کننده ندارد؛ درصد حضور تعریف نشده است.</p>'}${actions}</form>`);
    $('#edit-session').onclick = () => openEditor('sessions', id);
    $('#delete-session').onclick = () => confirmDelete('sessions', id);
    $('#attendance-form').addEventListener('submit', event => submit(event, form => api.attendance(id, [...form].map(([studentId, status]) => ({ studentId: Number(studentId), status })))));
}
function confirmDelete(type, id) {
    modal('حذف اطلاعات', `<form id="delete-form"><p>${type === 'sessions' ? 'این جلسه همراه با حضور و غیاب و ارزیابی‌های آن حذف می‌شود. ادامه می‌دهید؟' : 'این مورد حذف شود؟ افراد دارای سابقه قابل حذف نیستند؛ آن‌ها را غیرفعال کنید.'}</p>${actions.replace('ذخیره اطلاعات', 'حذف')}</form>`);
    $('#delete-form').addEventListener('submit', event => submit(event, () => api.remove(type, id)));
}
$('.navbar nav').addEventListener('click', event => { const b = event.target.closest('[data-tab]'); if (b) { state.tab = b.dataset.tab; state.page = 1; render(); } });
$('#filters').addEventListener('click', event => { const b = event.target.closest('[data-filter]'); if (b) { state.filter = b.dataset.filter; state.page = 1; render(); } });
$('#create').onclick = () => openEditor(state.tab);
$('#close-modal').onclick = () => $('#modal').close();
$('#modal').addEventListener('click', event => { if (event.target.closest('[data-cancel]')) $('#modal').close(); });
$('#previous').onclick = () => { state.page--; render(); };
$('#next').onclick = () => { state.page++; render(); };
$('#content').addEventListener('click', event => {
    const b = event.target.closest('button'); if (!b) return;
    if (b.hasAttribute('data-session')) openSession(Number(b.dataset.session));
    if (b.hasAttribute('data-edit')) openEditor(state.tab, Number(b.dataset.edit));
    if (b.hasAttribute('data-delete')) confirmDelete(state.tab, Number(b.dataset.delete));
    if (b.hasAttribute('data-retry')) refresh();
});
let resizeTimer;
addEventListener('resize', () => { clearTimeout(resizeTimer); resizeTimer = setTimeout(render, 150); });
if (isDemo) {
    $('#mode').textContent = 'نسخه نمایشی مستقل • به بک‌اند متصل نیست • ذخیره فقط در همین مرورگر';
    $('#reset-demo').hidden = false;
    $('#reset-demo').onclick = () => {
        modal('بازنشانی نسخه نمایشی', `<form id="reset-form"><p>تغییرات این مرورگر حذف و داده‌های ساختگی اولیه جایگزین شوند؟</p>${actions.replace('ذخیره اطلاعات', 'بازنشانی')}</form>`);
        $('#reset-form').addEventListener('submit', e => submit(e, async () => resetDemo()));
    };
}
refresh();
