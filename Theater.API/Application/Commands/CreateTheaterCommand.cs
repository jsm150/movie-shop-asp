using BuildingBlocks.API.Application;
using Theater.Domain.Aggregate;

namespace Theater.API.Application.Commands;

public record CreateTheaterCommand : ICommand<long>
{
    public required string Name { get; init; }
    public required int Floor { get; init; }
    public required TheaterType Type { get; init; }
    public required int RowCount { get; init; }
    public required int ColumnCount { get; init; }
    public required IReadOnlyCollection<string> Seats { get; init; }
}
