using BuildingBlocks.API.Application;
using Movie.Domain.Aggregate;

namespace Movie.API.Application.Commands;

public record ChangeMovieStatusCommand : ICommand<bool>
{
    public required long MovieId { get; init; }
    public required MovieStatus Status { get; init; }
}