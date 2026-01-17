using FluentValidation;
using MediatR;


namespace movie_shop_asp.Server.Application.Behaviors;


public class ValidatorBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validationTasks = validators.Select(v => v.ValidateAsync(request, cancellationToken));
        var validationResults = await Task.WhenAll(validationTasks);

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error != null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException("Validation exception", failures);
        }

        return await next(cancellationToken);
    }
}
