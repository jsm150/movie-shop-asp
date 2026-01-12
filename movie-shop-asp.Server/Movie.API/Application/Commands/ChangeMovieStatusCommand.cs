using MediatR;
using Movie.Domain.Aggregate;

namespace movie_shop_asp.Server.Movie.API.Application.Commands;

public record ChangeMovieStatusCommand : IRequest<bool>
{
    public required long MovieId { get; init; }
    public required MovieStatus Status { get; init; }
}