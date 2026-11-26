using BootCamp.Manager.Domain.Entities;
namespace BootCamp.Manager.Application.Interfaces;
public interface IEvaluationRepository
{
    Task<List<Evaluation>> GetAllAsync(CancellationToken ct);
    Task<Evaluation?> GetByIdAsync(int id, CancellationToken ct);
    void Add(Evaluation entity);
    void Remove(Evaluation entity);
    Task SaveChangesAsync(CancellationToken ct);
    
}
