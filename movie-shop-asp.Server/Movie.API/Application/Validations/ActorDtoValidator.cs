using FluentValidation;
using movie_shop_asp.Server.Movie.API.Application.Commands;

namespace movie_shop_asp.Server.Movie.API.Application.Validations
{
    public class ActorDtoValidator : AbstractValidator<ActorDto>
    {
        public ActorDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Cast member name is required.")
                .MaximumLength(100).WithMessage("Cast member name must not exceed 100 characters.");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTimeOffset.UtcNow).WithMessage("Cast member date of birth must be in the past.");

            RuleFor(x => x.National)
                .NotEmpty().WithMessage("Cast member nationality is required.")
                .MaximumLength(100).WithMessage("Cast member nationality must not exceed 100 characters.");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Cast member role is required.")
                .MaximumLength(100).WithMessage("Cast member role must not exceed 100 characters.");
        }
    }
}