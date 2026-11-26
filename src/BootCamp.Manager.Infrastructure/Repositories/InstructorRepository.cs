using BootCamp.Manager.Application.Interfaces;
using BootCamp.Manager.Domain.Entities;
using BootCamp.Manager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace BootCamp.Manager.Infrastructure.Repositories;
public class InstructorRepository(BootcampDbContext db) : IInstructorRepository
{
    public Task<List<Instructor>> GetAllAsync(CancellationToken ct) =>
        db.Instructors.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
    // دریافت تکی Tracking دارد تا ویرایش با SaveChanges ثبت شود.
    public Task<Instructor?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Instructors.FirstOrDefaultAsync(x => x.Id == id, ct);
    public void Add(Instructor entity) => db.Instructors.Add(entity);
    public void Remove(Instructor entity) => db.Instructors.Remove(entity);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public Task<bool> HasHistoryAsync(int id, CancellationToken ct) => HasInstructorHistoryAsync(id, ct);
    private async Task<bool> HasInstructorHistoryAsync(int id, CancellationToken ct) =>
        await db.Sessions.AnyAsync(x => x.InstructorId == id, ct) ||
        await db.Evaluations.AnyAsync(x => x.InstructorId == id, ct);
}
