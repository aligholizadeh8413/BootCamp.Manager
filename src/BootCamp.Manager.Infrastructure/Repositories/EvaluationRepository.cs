using BootCamp.Manager.Application.Interfaces;
using BootCamp.Manager.Domain.Entities;
using BootCamp.Manager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace BootCamp.Manager.Infrastructure.Repositories;
public class EvaluationRepository(BootcampDbContext db) : IEvaluationRepository
{
    public Task<List<Evaluation>> GetAllAsync(CancellationToken ct) =>
        db.Evaluations.AsNoTracking().Include(x => x.Instructor).Include(x => x.Participant).ThenInclude(p => p.Student).Include(x => x.Participant).ThenInclude(p => p.Session).OrderBy(x => x.Id).ToListAsync(ct);
    // دریافت تکی Tracking دارد تا ویرایش با SaveChanges ثبت شود.
    public Task<Evaluation?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Evaluations.Include(x => x.Instructor).Include(x => x.Participant).ThenInclude(p => p.Student).Include(x => x.Participant).ThenInclude(p => p.Session).FirstOrDefaultAsync(x => x.Id == id, ct);
    public void Add(Evaluation entity) => db.Evaluations.Add(entity);
    public void Remove(Evaluation entity) => db.Evaluations.Remove(entity);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    
}
