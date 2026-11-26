using BootCamp.Manager.Domain.Entities;
namespace BootCamp.Manager.Application.Interfaces;
public interface IInstructorRepository
{
    Task<List<Instructor>> GetAllAsync(CancellationToken ct);
    Task<Instructor?> GetByIdAsync(int id, CancellationToken ct);
    void Add(Instructor entity);
    void Remove(Instructor entity);
    Task SaveChangesAsync(CancellationToken ct);
    Task<bool> HasHistoryAsync(int id, CancellationToken ct);
}
