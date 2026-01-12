using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using movie_shop_asp.Server.Movie.API.Application.Commands;

namespace movie_shop_asp.Server.Movie.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController(IMediator mediator) : Controller
    {
        [HttpPost]
        public async Task<ActionResult> RegisterMovie(RegisterMovieCommand command, CancellationToken cancellationToken)
        {
            var ok = await mediator.Send(command, cancellationToken);
            return ok ? Ok() : StatusCode(500);
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteMovie(DeleteMovieCommand command, CancellationToken cancellationToken)
        {
            var ok = await mediator.Send(command, cancellationToken);
            return ok ? Ok() : StatusCode(500);
        }
    }
}
