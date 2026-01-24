
namespace BuildingBlocks.Domain;

public interface IRepository<T, ID>
where T : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }

    Task<T?> FindAsync(ID id);
    void Add(T value);
    void Remove(T value);
}
