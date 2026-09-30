using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;
using Mantaras.Juridico.Application.Features.Agenda.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Route("api/recordatorios-predeterminados")]
public sealed class RecordatoriosPredeterminadosController : ControllerBase
{
    private readonly IRecordatoriosPredeterminadosService _service;

    public RecordatoriosPredeterminadosController(
        IRecordatoriosPredeterminadosService service
    )
    {
        _service = service;
    }

    [HttpGet("tipos-entrada/{tipoEntradaAgendaId:long}")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<RecordatorioPredeterminadoResponse>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<
        ActionResult<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
    > ObtenerPorTipo(
        long tipoEntradaAgendaId,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.ObtenerPorTipoAsync(
            tipoEntradaAgendaId,
            cancellationToken
        );

        return result.IsFailure
            ? NotFound(CrearErrorResponse(result.Errors))
            : Ok(result.Value);
    }

    [HttpPut("tipos-entrada/{tipoEntradaAgendaId:long}")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<RecordatorioPredeterminadoResponse>),
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
    public async Task<
        ActionResult<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
    > GuardarPorTipo(
        long tipoEntradaAgendaId,
        [FromBody] GuardarRecordatoriosPredeterminadosRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.GuardarPorTipoAsync(
            tipoEntradaAgendaId,
            request,
            cancellationToken
        );

        return result.IsFailure
            ? NotFound(CrearErrorResponse(result.Errors))
            : Ok(result.Value);
    }

    [HttpGet("reglas-vencimiento/{reglaVencimientoId:long}")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<RecordatorioPredeterminadoResponse>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<
        ActionResult<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
    > ObtenerPorRegla(
        long reglaVencimientoId,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.ObtenerPorReglaAsync(
            reglaVencimientoId,
            cancellationToken
        );

        return result.IsFailure
            ? NotFound(CrearErrorResponse(result.Errors))
            : Ok(result.Value);
    }

    [HttpPut("reglas-vencimiento/{reglaVencimientoId:long}")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<RecordatorioPredeterminadoResponse>),
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
    public async Task<
        ActionResult<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>
    > GuardarPorRegla(
        long reglaVencimientoId,
        [FromBody] GuardarRecordatoriosPredeterminadosRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.GuardarPorReglaAsync(
            reglaVencimientoId,
            request,
            cancellationToken
        );

        return result.IsFailure
            ? NotFound(CrearErrorResponse(result.Errors))
            : Ok(result.Value);
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
}
