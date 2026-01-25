using BuildingBlocks.API.Application;
using MediatR;
using Movie.Domain.Aggregate;

namespace Movie.API.Application.Commands;

public record UpdateMovieCommand : ICommand<bool>
{
    public required long MovieId { get; init; }
    public required string Title { get; init; }
    public required string Director { get; init; }
    public required IReadOnlyCollection<string> Genres { get; init; }
    public required int RuntimeMinutes { get; init; }
    public required AdienceRating AdienceRating { get; init; }
    public required string Synopsis { get; init; }
    public required DateTimeOffset ReleaseDate { get; init; }
    public required IReadOnlyCollection<ActorDto> Casts { get; init; }
}