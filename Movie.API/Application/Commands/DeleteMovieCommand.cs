using BuildingBlocks.API.Application;

namespace Movie.API.Application.Commands;

public record DeleteMovieCommand(long MovieId) : ICommand<bool>;