// نسخه اصلی فقط با Fetch به API واقعی متصل می‌شود؛ هیچ fallback ساختگی ندارد.
export const isDemo = false;
export const resetDemo = () => {};
async function request(path, method = 'GET', body) {
    let response;
    try {
        response = await fetch(`/api/${path}`, {
            method,
            headers: body === undefined ? {} : { 'Content-Type': 'application/json' },
            body: body === undefined ? undefined : JSON.stringify(body)
        });
    } catch { throw new Error('ارتباط با سرور برقرار نشد. اجرای بک‌اند را بررسی کنید.'); }
    if (!response.ok) {
        const problem = await response.json().catch(() => ({}));
        const errors = problem.errors ? Object.values(problem.errors).flat().join(' / ') : '';
        throw new Error(errors || problem.title || `خطای درخواست (${response.status})`);
    }
    return response.status === 204 ? null : response.json();
}
export const api = {
    list: type => request(type),
    create: (type, data) => request(type, 'POST', data),
    update: (type, id, data) => request(`${type}/${id}`, 'PUT', data),
    remove: (type, id) => request(`${type}/${id}`, 'DELETE'),
    attendance: (id, items) => request(`sessions/${id}/attendance`, 'PUT', { items })
};
