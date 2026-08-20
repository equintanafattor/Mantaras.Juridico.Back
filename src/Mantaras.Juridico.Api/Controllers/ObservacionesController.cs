using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Observaciones.Requests;
using Mantaras.Juridico.Application.Features.Observaciones.Responses;
using Mantaras.Juridico.Application.Features.Observaciones.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class ObservacionesController : ControllerBase
{
    private readonly IObservacionesService _observacionesService;

    public ObservacionesController(
        IObservacionesService observacionesService
    )
    {
        _observacionesService = observacionesService;
    }

    [HttpGet("clientes/{clienteId:long}/observaciones")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<ObservacionResponse>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<
        ActionResult<IReadOnlyCollection<ObservacionResponse>>
    > ObtenerPorCliente(
        long clienteId,
        CancellationToken cancellationToken
    )
    {
        var result =
            await _observacionesService.ObtenerPorClienteAsync(
                clienteId,
                cancellationToken
            );

        return result.IsFailure
            ? NotFound(CrearErrorResponse(result.Errors))
            : Ok(result.Value);
    }

    [HttpPost("clientes/{clienteId:long}/observaciones")]
    [ProducesResponseType(
        typeof(ObservacionResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<ObservacionResponse>>
        CrearParaCliente(
            long clienteId,
            [FromBody] CrearObservacionRequest request,
            CancellationToken cancellationToken
        )
    {
        var result =
            await _observacionesService.CrearParaClienteAsync(
                clienteId,
                request,
                cancellationToken
            );

        if (result.IsFailure)
        {
            return NotFound(CrearErrorResponse(result.Errors));
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.Value
        );
    }

    [HttpGet("casos/{casoId:long}/observaciones")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<ObservacionResponse>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<
        ActionResult<IReadOnlyCollection<ObservacionResponse>>
    > ObtenerPorCaso(
        long casoId,
        CancellationToken cancellationToken
    )
    {
        var result = await _observacionesService.ObtenerPorCasoAsync(
            casoId,
            cancellationToken
        );

        return result.IsFailure
            ? NotFound(CrearErrorResponse(result.Errors))
            : Ok(result.Value);
    }

    [HttpPost("casos/{casoId:long}/observaciones")]
    [ProducesResponseType(
        typeof(ObservacionResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<ObservacionResponse>>
        CrearParaCaso(
            long casoId,
            [FromBody] CrearObservacionRequest request,
            CancellationToken cancellationToken
        )
    {
        var result = await _observacionesService.CrearParaCasoAsync(
            casoId,
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return NotFound(CrearErrorResponse(result.Errors));
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.Value
        );
    }

    [HttpGet("expedientes/{expedienteId:long}/observaciones")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<ObservacionResponse>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<
        ActionResult<IReadOnlyCollection<ObservacionResponse>>
    > ObtenerPorExpediente(
        long expedienteId,
        CancellationToken cancellationToken
    )
    {
        var result =
            await _observacionesService.ObtenerPorExpedienteAsync(
                expedienteId,
                cancellationToken
            );

        return result.IsFailure
            ? NotFound(CrearErrorResponse(result.Errors))
            : Ok(result.Value);
    }

    [HttpPost("expedientes/{expedienteId:long}/observaciones")]
    [ProducesResponseType(
        typeof(ObservacionResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<ObservacionResponse>>
        CrearParaExpediente(
            long expedienteId,
            [FromBody] CrearObservacionRequest request,
            CancellationToken cancellationToken
        )
    {
        var result =
            await _observacionesService.CrearParaExpedienteAsync(
                expedienteId,
                request,
                cancellationToken
            );

        if (result.IsFailure)
        {
            return NotFound(CrearErrorResponse(result.Errors));
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.Value
        );
    }

    private static ApiErrorResponse CrearErrorResponse(
        IEnumerable<Error> errors
    )
    {
        return new ApiErrorResponse
        {
            Errors = errors
                .Select(error => new ApiErrorItem
                {
                    Code = error.Code,
                    Message = error.Message,
                })
                .ToArray(),
        };
    }
}
