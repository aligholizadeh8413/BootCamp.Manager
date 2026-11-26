using BootCamp.Manager.Application.DTOs;
using BootCamp.Manager.Application.Exceptions;
using BootCamp.Manager.Application.Interfaces;
using BootCamp.Manager.Domain.Entities;
namespace BootCamp.Manager.Application.Services;

public class InstructorService(IInstructorRepository repository)
{
    public async Task<List<InstructorResponse>> GetAllAsync(CancellationToken ct) =>
        (await repository.GetAllAsync(ct)).Select(Map).ToList();
    public async Task<InstructorResponse> GetByIdAsync(int id, CancellationToken ct) => Map(await FindAsync(id, ct));
    public async Task<InstructorResponse> CreateAsync(InstructorRequest input, CancellationToken ct)
    {
        await CheckEmailAsync(input.Email, 0, ct);
        var entity = new Instructor();
        Apply(entity, input);
        repository.Add(entity);
        await repository.SaveChangesAsync(ct);
        return Map(entity);
    }
    public async Task UpdateAsync(int id, InstructorRequest input, CancellationToken ct)
    {
        var entity = await FindAsync(id, ct);
        await CheckEmailAsync(input.Email, id, ct);
        Apply(entity, input);
        await repository.SaveChangesAsync(ct);
    }
    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var entity = await FindAsync(id, ct);
        if (await repository.HasHistoryAsync(id, ct))
            throw new AppException(409, "این فرد سابقه دارد؛ برای حفظ آمار، او را غیرفعال کنید.");
        repository.Remove(entity);
        await repository.SaveChangesAsync(ct);
    }
    private async Task<Instructor> FindAsync(int id, CancellationToken ct) =>
        await repository.GetByIdAsync(id, ct) ?? throw new AppException(404, "فرد پیدا نشد.");
    private async Task CheckEmailAsync(string email, int id, CancellationToken ct)
    {
        if ((await repository.GetAllAsync(ct)).Any(x => x.Id != id && x.Email == email.Trim().ToLowerInvariant()))
            throw new AppException(409, "این ایمیل قبلاً ثبت شده است.");
    }
    private static void Apply(Instructor entity, InstructorRequest input)
    {
        entity.FullName = input.FullName.Trim();
        entity.Email = input.Email.Trim().ToLowerInvariant();
        entity.IsActive = input.IsActive;
        entity.Expertise = input.Expertise.Trim();
    }
    private static InstructorResponse Map(Instructor entity) => new(entity.Id, entity.FullName, entity.Email, entity.Expertise, entity.IsActive);
}
