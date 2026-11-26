using BootCamp.Manager.Application.DTOs;
using BootCamp.Manager.Application.Exceptions;
using BootCamp.Manager.Application.Interfaces;
using BootCamp.Manager.Domain.Entities;
namespace BootCamp.Manager.Application.Services;
public class EvaluationService(IEvaluationRepository evaluations, ISessionRepository sessions,
    IInstructorRepository instructors)
{
    public async Task<List<EvaluationResponse>> GetAllAsync(CancellationToken ct) =>
        (await evaluations.GetAllAsync(ct)).Select(Map).ToList();
    public async Task<EvaluationResponse> GetByIdAsync(int id, CancellationToken ct) => Map(await FindAsync(id, ct));
    public async Task<EvaluationResponse> CreateAsync(EvaluationRequest input, CancellationToken ct)
    {
        await ValidateAsync(input, 0, ct);
        var entity = new Evaluation();
        Apply(entity, input);
        evaluations.Add(entity);
        await evaluations.SaveChangesAsync(ct);
        return await GetByIdAsync(entity.Id, ct);
    }
    public async Task UpdateAsync(int id, EvaluationRequest input, CancellationToken ct)
    {
        var entity = await FindAsync(id, ct);
        await ValidateAsync(input, id, ct);
        Apply(entity, input);
        await evaluations.SaveChangesAsync(ct);
    }
    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        evaluations.Remove(await FindAsync(id, ct));
        await evaluations.SaveChangesAsync(ct);
    }
    private async Task<Evaluation> FindAsync(int id, CancellationToken ct) =>
        await evaluations.GetByIdAsync(id, ct) ?? throw new AppException(404, "ارزیابی پیدا نشد.");
    private async Task ValidateAsync(EvaluationRequest input, int id, CancellationToken ct)
    {
        var session = await sessions.GetByIdAsync(input.SessionId, ct);
        if (session is null || !session.Participants.Any(p => p.StudentId == input.StudentId))
            throw new AppException(400, "دانشجو باید در فهرست شرکت‌کنندگان این جلسه باشد.");
        if (await instructors.GetByIdAsync(input.InstructorId, ct) is null)
            throw new AppException(400, "مدرس پیدا نشد.");
        if ((await evaluations.GetAllAsync(ct)).Any(e => e.Id != id && e.SessionId == input.SessionId && e.StudentId == input.StudentId))
            throw new AppException(409, "ارزیابی این دانشجو در جلسه ثبت شده؛ آن را ویرایش کنید.");
    }
    private static void Apply(Evaluation e, EvaluationRequest input)
    {
        e.SessionId = input.SessionId; e.StudentId = input.StudentId;
        e.InstructorId = input.InstructorId; e.Score = input.Score; e.Notes = input.Notes.Trim();
    }
    private static EvaluationResponse Map(Evaluation e) => new(e.Id, e.SessionId,
        e.Participant.Session.Title, e.StudentId, e.Participant.Student.FullName,
        e.InstructorId, e.Instructor.FullName, e.Score, e.Notes);
}
