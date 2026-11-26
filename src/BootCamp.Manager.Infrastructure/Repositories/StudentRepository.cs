using BootCamp.Manager.Application.Interfaces;
using BootCamp.Manager.Domain.Entities;
using BootCamp.Manager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace BootCamp.Manager.Infrastructure.Repositories;
public class StudentRepository(BootcampDbContext db) : IStudentRepository
{
    public Task<List<Student>> GetAllAsync(CancellationToken ct) =>
        db.Students.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
    // دریافت تکی Tracking دارد تا ویرایش با SaveChanges ثبت شود.
    public Task<Student?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Students.FirstOrDefaultAsync(x => x.Id == id, ct);
    public void Add(Student entity) => db.Students.Add(entity);
    public void Remove(Student entity) => db.Students.Remove(entity);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public Task<bool> HasHistoryAsync(int id, CancellationToken ct) => db.SessionParticipants.AnyAsync(x => x.StudentId == id, ct);
}
