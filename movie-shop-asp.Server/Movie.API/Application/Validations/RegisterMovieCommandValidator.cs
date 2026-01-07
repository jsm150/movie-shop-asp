using FluentValidation;
using movie_shop_asp.Server.Movie.API.Application.Commands;

namespace movie_shop_asp.Server.Movie.API.Application.Validations
{
    public class RegisterMovieCommandValidator : AbstractValidator<RegisterMovieCommand>
    {
        public RegisterMovieCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Director)
                .NotEmpty().WithMessage("Director is required.")
                .MaximumLength(100).WithMessage("Director must not exceed 100 characters.");

            RuleFor(x => x.Genres)
                .NotEmpty().WithMessage("At least one genre is required.")
                .Must(genres => genres.All(g => !string.IsNullOrWhiteSpace(g)))
                .WithMessage("Genres cannot contain empty or whitespace values.");

            RuleFor(x => x.RuntimeMinutes)
                .GreaterThan(0).WithMessage("RuntimeMinutes must be greater than 0.");

            RuleFor(x => x.Synopsis)
                .NotEmpty().WithMessage("Synopsis is required.")
                .MaximumLength(1000).WithMessage("Synopsis must not exceed 1000 characters.");

            RuleFor(x => x.ReleaseDate)
                .GreaterThan(DateTime.MinValue).WithMessage("ReleaseDate is required.");

            RuleFor(x => x.Casts)
                .NotEmpty().WithMessage("At least one cast member is required.");

            RuleForEach(x => x.Casts).SetValidator(new ActorDtoValidator());

            RuleFor(x => x.AdienceRating)
                .IsInEnum().WithMessage("AdienceRating must be a valid value.");
            
        }
    }
}
