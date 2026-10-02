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
[Route("api/recordatorios-agenda")]
public sealed class RecordatoriosAgendaController : ControllerBase
{
    private readonly IRecordatoriosAgendaService _recordatoriosService;

    public RecordatoriosAgendaController(
        IRecordatoriosAgendaService recordatoriosService
    )
    {
        _recordatoriosService = recordatoriosService;
    }

    [HttpPost("entrada/{entradaAgendaId:long}")]
    [ProducesResponseType(
        typeof(RecordatorioAgendaResponse),
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
    public async Task<ActionResult<RecordatorioAgendaResponse>> Crear(
        long entradaAgendaId,
        [FromBody] CrearRecordatorioAgendaRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _recordatoriosService.CrearAsync(
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

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResponse<RecordatorioAgendaResponse>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<PagedResponse<RecordatorioAgendaResponse>>>
        Buscar(
            [FromQuery] BuscarRecordatoriosAgendaRequest request,
            CancellationToken cancellationToken
        )
    {
        return Ok(
            await _recordatoriosService.BuscarAsync(
                request,
                cancellationToken
            )
        );
    }

    [HttpPatch("{recordatorioAgendaId:long}/atender")]
    [ProducesResponseType(
        typeof(RecordatorioAgendaResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<RecordatorioAgendaResponse>> Atender(
        long recordatorioAgendaId,
        CancellationToken cancellationToken
    )
    {
        var result = await _recordatoriosService.AtenderAsync(
            recordatorioAgendaId,
            cancellationToken
        );

        return result.IsFailure
            ? NotFound(CrearErrorResponse(result.Errors))
            : Ok(result.Value);
    }

    [HttpPut("{recordatorioAgendaId:long}")]
    [ProducesResponseType(typeof(RecordatorioAgendaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecordatorioAgendaResponse>> Reprogramar(
        long recordatorioAgendaId,
        [FromBody] CrearRecordatorioAgendaRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _recordatoriosService.ReprogramarAsync(recordatorioAgendaId, request, cancellationToken);
        if (result.IsFailure)
        {
            var response = CrearErrorResponse(result.Errors);
            return ContieneError(result.Errors, AgendaErrors.RecordatorioNoEncontrado) ? NotFound(response) : BadRequest(response);
        }
        return Ok(result.Value);
    }

    [HttpDelete("{recordatorioAgendaId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Quitar(long recordatorioAgendaId, CancellationToken cancellationToken)
    {
        var result = await _recordatoriosService.QuitarAsync(recordatorioAgendaId, cancellationToken);
        if (result.IsFailure)
        {
            var response = CrearErrorResponse(result.Errors);
            return ContieneError(result.Errors, AgendaErrors.RecordatorioNoEncontrado) ? NotFound(response) : BadRequest(response);
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
        return errors.Any(error => error.Code == expectedError.Code);
    }
}
