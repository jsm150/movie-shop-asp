using MediatR;
using Movie.Domain.Aggregate;

namespace movie_shop_asp.Server.Movie.API.Application.Commands
{
    public record RegisterMovieCommand : IRequest<bool>
    {
        public required string Title { get; init; }
        public required string Director { get; init; }
        public required IReadOnlyCollection<string> Genres { get; init; }
        public required int RuntimeMinutes { get; init; }
        public required AdienceRating AdienceRating { get; init; }
        public required string Synopsis { get; init; }
        public required DateTimeOffset ReleaseDate { get; init; }
        public required IReadOnlyCollection<ActorDto> Casts { get; init; }
    }

    public record ActorDto
    {
        public required string Name { get; init; }
        public required DateTimeOffset DateOfBirth { get; init; }
        public required string National { get; init; }
        public required string Role { get; init; }
    }
}
