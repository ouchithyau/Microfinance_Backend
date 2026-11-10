using System.Net;
using System.Text.Json;
using FluentValidation;
using Microfinance.Application.Common.Models;
using Microfinance.Domain.Exceptions;

namespace Microfinance.WebApi.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException valEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Result = Result.Failure(valEx.Errors.Select(e => e.ErrorMessage).ToArray())
            },
            DomainException domEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Result = Result.Failure(domEx.Message)
            },
            KeyNotFoundException notFoundEx => new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Result = Result.Failure(notFoundEx.Message)
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Result = Result.Failure("An internal server error occurred. Please contact administrator.")
            }
        };

        context.Response.StatusCode = response.StatusCode;
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response.Result, jsonOptions));
    }
}