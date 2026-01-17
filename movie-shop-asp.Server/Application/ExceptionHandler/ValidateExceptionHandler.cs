using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace movie_shop_asp.Server.Application.ExceptionHandler;

public class ValidateExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var problemDetails = new ValidationProblemDetails
        {
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        foreach (var error in validationException.Errors)
        {
            problemDetails.Errors.Add(error.PropertyName, [error.ErrorMessage]);
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

