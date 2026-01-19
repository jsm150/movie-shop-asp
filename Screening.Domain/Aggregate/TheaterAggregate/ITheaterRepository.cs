using BuildingBlocks.Domain;

namespace Screening.Domain.Aggregate.TheaterAggregate;

public interface ITheaterRepository : IRepository<Theater>
{
    Task<Theater?> GetAsync(long theaterId, CancellationToken cancellationToken);
    void Add(Theater theater);
}