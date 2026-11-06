using System.Net;
using System.Text.Json;
using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mantaras.Juridico.Api.Middleware;

// Las respuestas de error usan el mismo formato que los controladores:
// { "errors": [ { "code": "...", "message": "..." } ] }
public class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // El cliente cortó la solicitud (navegación o consulta cancelada en el front).
            // No es un error del servidor y ya no hay a quién responder.
            _logger.LogDebug("Solicitud cancelada por el cliente: {Path}", context.Request.Path);
        }
        catch (BusinessException exception)
        {
            await EscribirRespuestaAsync(
                context,
                HttpStatusCode.BadRequest,
                "General.ReglaDeNegocio",
                exception.Message
            );
        }
        catch (Exception exception) when (EsRegistroConcurrente(exception))
        {
            // Un índice único rechazó el guardado y el servicio no pudo resolverlo.
            _logger.LogWarning(exception, "Conflicto de unicidad al guardar.");

            await EscribirRespuestaAsync(
                context,
                HttpStatusCode.Conflict,
                "General.RegistroConcurrente",
                "Otra operación registró los mismos datos al mismo tiempo. Actualice la pantalla y vuelva a intentarlo."
            );
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Ocurrió un error no controlado.");

            await EscribirRespuestaAsync(
                context,
                HttpStatusCode.InternalServerError,
                "General.ErrorInterno",
                "Ocurrió un error interno."
            );
        }
    }

    private static bool EsRegistroConcurrente(Exception exception) =>
        exception is RegistroConcurrenteException
        || exception is DbUpdateException
        {
            InnerException: PostgresException { SqlState: PostgresErrorCodes.UniqueViolation },
        };

    private static async Task EscribirRespuestaAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string codigo,
        string mensaje
    )
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiErrorResponse
        {
            Errors = new[] { new ApiErrorItem { Code = codigo, Message = mensaje } },
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
