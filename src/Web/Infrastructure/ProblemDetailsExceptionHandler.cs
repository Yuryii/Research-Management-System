using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RMS.Application.Common.Exceptions;

namespace RMS.Web.Infrastructure;
/// <summary>
/// Converts well-known application exceptions into RFC 9110-compliant <see cref="ProblemDetails"/> responses,
/// mapping <see cref="ValidationException"/> → 400, <see cref="NotFoundException"/> → 404,
/// <see cref="UnauthorizedAccessException"/> → 401, and <see cref="ForbiddenAccessException"/> → 403.
/// Unrecognised exceptions are not handled and fall through to the default middleware.
/// </summary>
public class ProblemDetailsExceptionHandler() : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest, new ValidationProblemDetails(ve.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
            }),
            NotFoundException ne => (StatusCodes.Status404NotFound, new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Title = "The specified resource was not found.",
                Detail = ne.Message
            }),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2"
            }),
            ForbiddenAccessException => (StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4"
            }),
            _ => (-1, null)
        };

        if (problemDetails is null) return false;
        httpContext.Response.StatusCode = statusCode;
        switch (problemDetails)
        {
            case ValidationProblemDetails validationProblemDetails:
                await httpContext.Response.WriteAsJsonAsync(validationProblemDetails, cancellationToken);
                break;
            default:
                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
                break;
        }

        return true;
    }
}
