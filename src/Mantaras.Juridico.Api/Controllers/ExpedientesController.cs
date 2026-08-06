using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Expedientes;
using Mantaras.Juridico.Application.Features.Expedientes.Requests;
using Mantaras.Juridico.Application.Features.Expedientes.Responses;
using Mantaras.Juridico.Application.Features.Expedientes.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Route("api/expedientes")]
public sealed class ExpedientesController : ControllerBase
{
    private readonly IExpedientesService _expedientesService;

    public ExpedientesController(
        IExpedientesService expedientesService
    )
    {
        _expedientesService = expedientesService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ExpedienteResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    public async Task<ActionResult<ExpedienteResponse>> Crear(
        [FromBody] CrearExpedienteRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _expedientesService.CrearAsync(
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return BadRequest(CrearErrorResponse(result.Errors));
        }

        var expediente = result.Value!;

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { expedienteId = expediente.ExpedienteId },
            expediente
        );
    }

    [HttpGet("{expedienteId:long}")]
    [ProducesResponseType(
        typeof(ExpedienteDetalleResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<ExpedienteDetalleResponse>> ObtenerPorId(
        long expedienteId,
        CancellationToken cancellationToken
    )
    {
        var result = await _expedientesService.ObtenerPorIdAsync(
            expedienteId,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return NotFound(CrearErrorResponse(result.Errors));
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResponse<ExpedienteResponse>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<PagedResponse<ExpedienteResponse>>> Buscar(
        [FromQuery] BuscarExpedientesRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _expedientesService.BuscarAsync(
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPut("{expedienteId:long}")]
    [ProducesResponseType(
        typeof(ExpedienteResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<ExpedienteResponse>> Actualizar(
        long expedienteId,
        [FromBody] ActualizarExpedienteRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _expedientesService.ActualizarAsync(
            expedienteId,
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            var errorResponse = CrearErrorResponse(result.Errors);

            if (ContieneError(
                result.Errors,
                ExpedienteErrors.NoEncontrado
            ))
            {
                return NotFound(errorResponse);
            }

            return BadRequest(errorResponse);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{expedienteId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> DarDeBaja(
        long expedienteId,
        CancellationToken cancellationToken
    )
    {
        var result = await _expedientesService.DarDeBajaAsync(
            expedienteId,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return NotFound(CrearErrorResponse(result.Errors));
        }

        return NoContent();
    }

    [HttpPatch("{expedienteId:long}/restaurar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<IActionResult> Restaurar(
        long expedienteId,
        CancellationToken cancellationToken
    )
    {
        var result = await _expedientesService.RestaurarAsync(
            expedienteId,
            cancellationToken
        );

        if (result.IsFailure)
        {
            var errorResponse = CrearErrorResponse(result.Errors);

            if (ContieneError(
                result.Errors,
                ExpedienteErrors.NoEncontrado
            ))
            {
                return NotFound(errorResponse);
            }

            return BadRequest(errorResponse);
        }

        return NoContent();
    }

    private static ApiErrorResponse CrearErrorResponse(
        IReadOnlyCollection<Error> errors
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

    private static bool ContieneError(
        IReadOnlyCollection<Error> errors,
        Error expectedError
    )
    {
        return errors.Any(
            error => error.Code == expectedError.Code
        );
    }
}