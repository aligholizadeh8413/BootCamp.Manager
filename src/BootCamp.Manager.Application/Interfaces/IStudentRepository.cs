using BootCamp.Manager.Domain.Entities;
namespace BootCamp.Manager.Application.Interfaces;
public interface IStudentRepository
{
    Task<List<Student>> GetAllAsync(CancellationToken ct);
    Task<Student?> GetByIdAsync(int id, CancellationToken ct);
    void Add(Student entity);
    void Remove(Student entity);
    Task SaveChangesAsync(CancellationToken ct);
    Task<bool> HasHistoryAsync(int id, CancellationToken ct);
}
