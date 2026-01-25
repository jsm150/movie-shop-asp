using BuildingBlocks.API.Application;
using MediatR;

namespace Screening.API.Application.Commands;

public sealed record UpdateScreenCommand : ICommand<bool>
{
    public required long ScreenId { get; init; }
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }
    public required DateTimeOffset SalesStartAt { get; init; }
    public required DateTimeOffset SalesEndAt { get; init; }
}