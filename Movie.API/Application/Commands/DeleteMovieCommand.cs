using MediatR;

namespace Movie.API.Application.Commands;

public record DeleteMovieCommand(long MovieId) : IRequest<bool>;