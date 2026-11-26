using BootCamp.Manager.Application.DTOs;
using BootCamp.Manager.Application.Exceptions;
using BootCamp.Manager.Application.Interfaces;
using BootCamp.Manager.Domain.Entities;
namespace BootCamp.Manager.Application.Services;

public class SessionService(ISessionRepository sessions, IStudentRepository students,
    IInstructorRepository instructors)
{
    public async Task<List<SessionResponse>> GetAllAsync(CancellationToken ct) =>
        (await sessions.GetAllAsync(ct)).Select(Map).ToList();
    public async Task<SessionResponse> GetByIdAsync(int id, CancellationToken ct) => Map(await FindAsync(id, ct));
    public async Task<SessionResponse> CreateAsync(CreateSessionRequest input, CancellationToken ct)
    {
        await ValidateInstructorAsync(input.InstructorId, null, ct);
        if (input.StudentIds.Count != input.StudentIds.Distinct().Count())
            throw new AppException(400, "شناسه دانشجو نباید تکراری باشد.");
        var available = await students.GetAllAsync(ct);
        if (input.StudentIds.Any(id => !available.Any(s => s.Id == id && s.IsActive)))
            throw new AppException(400, "فقط دانشجوی موجود و فعال را انتخاب کنید.");
        var session = new Session();
        Apply(session, input);
        // فهرست مستقل جلسه: افزودن دانشجوی جدید این مجموعه را تغییر نمی‌دهد.
        session.Participants = input.StudentIds.Select(id => new SessionParticipant { StudentId = id }).ToList();
        sessions.Add(session);
        await sessions.SaveChangesAsync(ct);
        return await GetByIdAsync(session.Id, ct);
    }
    public async Task UpdateAsync(int id, SessionRequest input, CancellationToken ct)
    {
        var session = await FindAsync(id, ct);
        await ValidateInstructorAsync(input.InstructorId, session.InstructorId, ct);
        Apply(session, input); // ویرایش مشخصات جلسه، شرکت‌کنندگان را بازنویسی نمی‌کند.
        await sessions.SaveChangesAsync(ct);
    }
    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        sessions.Remove(await FindAsync(id, ct));
        await sessions.SaveChangesAsync(ct);
    }
    public async Task SetAttendanceAsync(int id, AttendanceRequest input, CancellationToken ct)
    {
        var session = await FindAsync(id, ct);
        // PUT جایگزین کل وضعیت حضور جلسه است؛ ابتدا همه ورودی‌ها بررسی می‌شوند.
        if (input.Items.Any(x => x is null || x.Status is null || !Enum.IsDefined(x.Status.Value)))
            throw new AppException(400, "وضعیت حضور نامعتبر است.");
        var ids = input.Items.Select(x => x.StudentId).ToList();
        if (ids.Count != ids.Distinct().Count() ||
            !ids.Order().SequenceEqual(session.Participants.Select(p => p.StudentId).Order()))
            throw new AppException(400, "برای هر شرکت‌کننده دقیقاً یک وضعیت ارسال کنید.");
        foreach (var participant in session.Participants)
            participant.Status = input.Items.Single(x => x.StudentId == participant.StudentId).Status!.Value;
        await sessions.SaveChangesAsync(ct);
    }
    private async Task<Session> FindAsync(int id, CancellationToken ct) =>
        await sessions.GetByIdAsync(id, ct) ?? throw new AppException(404, "جلسه پیدا نشد.");
    private async Task ValidateInstructorAsync(int id, int? previousId, CancellationToken ct)
    {
        var instructor = await instructors.GetByIdAsync(id, ct);
        if (instructor is null || (!instructor.IsActive && previousId != id))
            throw new AppException(400, "مدرس فعال و معتبر انتخاب کنید.");
    }
    private static void Apply(Session session, SessionRequest input)
    {
        if (input.Date is null || input.Date.Value == DateOnly.MinValue)
            throw new AppException(400, "تاریخ معتبر وارد کنید.");
        session.Title = input.Title.Trim();
        session.Date = input.Date.Value;
        session.InstructorId = input.InstructorId;
        session.TaskDescription = input.TaskDescription.Trim();
        session.ProgressPercent = input.ProgressPercent;
        session.IsCompleted = input.IsCompleted;
    }
    private static SessionResponse Map(Session s)
    {
        int total = s.Participants.Count;
        int present = s.Participants.Count(p => p.Status == AttendanceStatus.Present);
        return new(s.Id, s.Title, s.Date, s.InstructorId, s.Instructor.FullName,
            s.TaskDescription, s.ProgressPercent, s.IsCompleted, total, present,
            s.Participants.Count(p => p.Status == AttendanceStatus.Pending),
            total == 0 ? null : Math.Round(present * 100.0 / total, 1),
            s.Participants.OrderBy(p => p.StudentId)
                .Select(p => new ParticipantResponse(p.StudentId, p.Student.FullName, p.Status)).ToList());
    }
}
