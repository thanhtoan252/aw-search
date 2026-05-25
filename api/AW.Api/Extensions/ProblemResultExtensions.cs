using AW.Domain.Common;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace AW.Api.Extensions;

internal static class ProblemResultExtensions
{
    internal static ProblemDetails ToProblemDetails(this Error error)
    {
        var problemDetails = new ProblemDetails
        {
            Status = GetStatusCode(error.Type),
            Title = GetTitle(error.Type),
            Detail = error.Description,
            Extensions =
            {
                ["code"] = error.Code
            }
        };

        return problemDetails;
    }

    internal static IResult ToValidationProblem(this ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return Results.ValidationProblem(
            errors,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Validation failed",
            detail: "One or more validation errors occurred.",
            extensions: new Dictionary<string, object?>
            {
                ["code"] = "Validation.Failed"
            });
    }

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.ExternalDependency => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError
        };

    private static string GetTitle(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => "Validation failed",
            ErrorType.NotFound => "Resource not found",
            ErrorType.Conflict => "Request conflict",
            ErrorType.ExternalDependency => "External dependency failed",
            _ => "Request failed"
        };
}
