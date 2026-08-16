using BikeStore.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (status, title, detail) =
            exception switch
            {
                NotFoundException =>
                    (
                        StatusCodes.Status404NotFound,
                        "Recurso no encontrado",
                        exception.Message
                    ),

                BusinessException =>
                    (
                        StatusCodes.Status400BadRequest,
                        "Solicitud no válida",
                        exception.Message
                    ),

                ConflictException =>
                    (
                        StatusCodes.Status409Conflict,
                        "Conflicto con los datos",
                        exception.Message
                    ),

                _ =>
                    (
                        StatusCodes.Status500InternalServerError,
                        "Error interno del servidor",
                        "Ocurrió un error inesperado."
                    )
            };

        if (status >= 500)
        {
            logger.LogError(
                exception,
                "Error inesperado. TraceId: {TraceId}",
                context.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(
                "Solicitud rechazada: {Message}. TraceId: {TraceId}",
                exception.Message,
                context.TraceIdentifier);
        }

        context.Response.StatusCode = status;
        context.Response.ContentType =
            "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] =
            context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problem);
    }
}