using BuildingBlocks.API.Application;


namespace Screening.API.Application.Commands;

public sealed record CreateScreenCommand : ICommand<long>
{
    public required long MovieId { get; init; }
    public required long TheaterId { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }
    public required DateTimeOffset SalesStartAt { get; init; }
    public required DateTimeOffset SalesEndAt { get; init; }
}