using BootCamp.Manager.Domain.Entities;
namespace BootCamp.Manager.Application.Interfaces;
public interface ISessionRepository
{
    Task<List<Session>> GetAllAsync(CancellationToken ct);
    Task<Session?> GetByIdAsync(int id, CancellationToken ct);
    void Add(Session entity);
    void Remove(Session entity);
    Task SaveChangesAsync(CancellationToken ct);
    
}
