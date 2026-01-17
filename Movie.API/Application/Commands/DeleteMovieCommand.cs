using MediatR;

namespace movie_shop_asp.Server.Movie.API.Application.Commands;

public record DeleteMovieCommand(long MovieId) : IRequest<bool>;