using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions.Handler;
public class ExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Log the error
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        (string Detail, string Title, int StatusCode) details = exception switch
        {
            InternalServerException =>
            (
                exception.Message,
                exception.GetType().Name,
                context.Response.StatusCode = StatusCodes.Status500InternalServerError
            ),
            ValidationException =>
            (
                exception.Message,
                exception.GetType().Name,
                context.Response.StatusCode = StatusCodes.Status400BadRequest
            ),
            BadRequestException =>
            (
                exception.Message,
                exception.GetType().Name,
                context.Response.StatusCode = StatusCodes.Status400BadRequest
            ),
            NotFoundException =>
            (
                exception.Message,
                exception.GetType().Name,
                context.Response.StatusCode = StatusCodes.Status404NotFound
            ),
            _ =>
            (
                exception.Message,
                exception.GetType().Name,
                context.Response.StatusCode = StatusCodes.Status500InternalServerError
            )
        };

        // Create the ProblemDetails object
        var problemDetails = new ProblemDetails
        {
            Title = details.Title,
            Status = details.StatusCode,
            Detail = details.Detail,
            Instance = context.Request.Path,
        };

        problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

        //If exception is validation exception
        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions.Add("ValidationErrors", validationException.Errors);
        }

        // Set response properties
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        // Write the response
        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Indicate that the exception has been handled
        return true;
    }
}
