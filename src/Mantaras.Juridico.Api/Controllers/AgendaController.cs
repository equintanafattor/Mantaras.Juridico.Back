using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;
using Mantaras.Juridico.Application.Features.Agenda.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Route("api/agenda")]
public sealed class AgendaController : ControllerBase
{
    private readonly IAgendaService _agendaService;

    public AgendaController(IAgendaService agendaService)
    {
        _agendaService = agendaService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(EntradaAgendaResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    public async Task<ActionResult<EntradaAgendaResponse>> Crear(
        [FromBody] GuardarEntradaAgendaRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _agendaService.CrearAsync(
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return BadRequest(CrearErrorResponse(result.Errors));
        }

        var entrada = result.Value!;

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { entradaAgendaId = entrada.EntradaAgendaId },
            entrada
        );
    }

    [HttpPost("vencimientos")]
    [ProducesResponseType(
        typeof(EntradaAgendaResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    public async Task<ActionResult<EntradaAgendaResponse>> CrearVencimientoManual(
        [FromBody] GuardarEntradaAgendaRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _agendaService.CrearVencimientoManualAsync(
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return BadRequest(CrearErrorResponse(result.Errors));
        }

        var entrada = result.Value!;

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { entradaAgendaId = entrada.EntradaAgendaId },
            entrada
        );
    }

    [HttpGet("{entradaAgendaId:long}")]
    [ProducesResponseType(
        typeof(EntradaAgendaResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<EntradaAgendaResponse>> ObtenerPorId(
        long entradaAgendaId,
        CancellationToken cancellationToken
    )
    {
        var result = await _agendaService.ObtenerPorIdAsync(
            entradaAgendaId,
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
        typeof(PagedResponse<EntradaAgendaListadoResponse>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<PagedResponse<EntradaAgendaListadoResponse>>> Buscar(
        [FromQuery] BuscarAgendaRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _agendaService.BuscarAsync(
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPut("{entradaAgendaId:long}")]
    [ProducesResponseType(
        typeof(EntradaAgendaResponse),
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
    public async Task<ActionResult<EntradaAgendaResponse>> Actualizar(
        long entradaAgendaId,
        [FromBody] GuardarEntradaAgendaRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _agendaService.ActualizarAsync(
            entradaAgendaId,
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            var errorResponse = CrearErrorResponse(result.Errors);

            return ContieneError(result.Errors, AgendaErrors.NoEncontrada)
                ? NotFound(errorResponse)
                : BadRequest(errorResponse);
        }

        return Ok(result.Value);
    }

    [HttpPatch("{entradaAgendaId:long}/estado")]
    [ProducesResponseType(
        typeof(EntradaAgendaResponse),
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
    public async Task<ActionResult<EntradaAgendaResponse>> CambiarEstado(
        long entradaAgendaId,
        [FromBody] CambiarEstadoAgendaRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _agendaService.CambiarEstadoAsync(
            entradaAgendaId,
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            var errorResponse = CrearErrorResponse(result.Errors);

            return ContieneError(result.Errors, AgendaErrors.NoEncontrada)
                ? NotFound(errorResponse)
                : BadRequest(errorResponse);
        }

        return Ok(result.Value);
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
        return errors.Any(error => error.Code == expectedError.Code);
    }
}
