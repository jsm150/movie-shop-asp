using MediatR;
using Microsoft.AspNetCore.Mvc;
using Movie.API.Application.Commands;
using movie_shop_asp.Server.Movie.API.Application.Commands;

namespace Movie.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovieController(IMediator mediator) : Controller
{
    [HttpPost]
    public async Task<ActionResult> RegisterMovie(RegisterMovieCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateMovie(UpdateMovieCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteMovie(DeleteMovieCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Ok();
    }

    [HttpPut("status")]
    public async Task<ActionResult> ChangeMovieStatus(ChangeMovieStatusCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Ok();
    }
}
