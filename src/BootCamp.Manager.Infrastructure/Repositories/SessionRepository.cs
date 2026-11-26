using BootCamp.Manager.Application.Interfaces;
using BootCamp.Manager.Domain.Entities;
using BootCamp.Manager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace BootCamp.Manager.Infrastructure.Repositories;
public class SessionRepository(BootcampDbContext db) : ISessionRepository
{
    public Task<List<Session>> GetAllAsync(CancellationToken ct) =>
        db.Sessions.AsNoTracking().Include(x => x.Instructor).Include(x => x.Participants).ThenInclude(p => p.Student).OrderBy(x => x.Id).ToListAsync(ct);
    // دریافت تکی Tracking دارد تا ویرایش با SaveChanges ثبت شود.
    public Task<Session?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Sessions.Include(x => x.Instructor).Include(x => x.Participants).ThenInclude(p => p.Student).FirstOrDefaultAsync(x => x.Id == id, ct);
    public void Add(Session entity) => db.Sessions.Add(entity);
    public void Remove(Session entity) => db.Sessions.Remove(entity);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    
}
