using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.DiasInhabiles;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Responses;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dias-inhabiles")]
public class DiasInhabilesController : ControllerBase
{
    private readonly IDiasInhabilesService _service;
    private readonly IImportacionFeriadosService _importacionService;

    public DiasInhabilesController(
        IDiasInhabilesService service,
        IImportacionFeriadosService importacionService
    )
    {
        _service = service;
        _importacionService = importacionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DiaInhabilResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<DiaInhabilResponse>>> Buscar(
        [FromQuery] BuscarDiasInhabilesRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.BuscarAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(DiaInhabilResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiaInhabilResponse>> ObtenerPorId(
        long id,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.ObtenerPorIdAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DiaInhabilResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DiaInhabilResponse>> Crear(
        [FromBody] GuardarDiaInhabilRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.CrearAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        var entidad = result.Value!;

        return CreatedAtAction(nameof(ObtenerPorId), new { id = entidad.DiaInhabilId }, entidad);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(DiaInhabilResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DiaInhabilResponse>> Actualizar(
        long id,
        [FromBody] GuardarDiaInhabilRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.ActualizarAsync(id, request, cancellationToken);

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DarDeBaja(long id, CancellationToken cancellationToken)
    {
        var result = await _service.DarDeBajaAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return NoContent();
    }

    [HttpPatch("{id:long}/reactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivar(long id, CancellationToken cancellationToken)
    {
        var result = await _service.ReactivarAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return NoContent();
    }

    // Consulta la fuente oficial y compara con lo guardado. No modifica datos.
    [HttpGet("importacion-nacional/{anio:int}/vista-previa")]
    [ProducesResponseType(typeof(VistaPreviaImportacionFeriadosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<VistaPreviaImportacionFeriadosResponse>> VistaPreviaImportacion(
        int anio,
        CancellationToken cancellationToken
    )
    {
        var result = await _importacionService.ObtenerVistaPreviaAsync(anio, cancellationToken);

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return Ok(result.Value);
    }

    // Aplica la vista previa identificada por la huella. Repetirla con la misma
    // huella devuelve el resultado ya aplicado (yaAplicada = true).
    [HttpPost("importacion-nacional/{anio:int}")]
    [ProducesResponseType(typeof(ConfirmacionImportacionFeriadosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ConfirmacionImportacionFeriadosResponse>> ConfirmarImportacion(
        int anio,
        [FromBody] ConfirmarImportacionFeriadosRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _importacionService.ConfirmarAsync(anio, request, cancellationToken);

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpGet("importacion-nacional/estado")]
    [ProducesResponseType(typeof(EstadoCalendarioFeriadosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EstadoCalendarioFeriadosResponse>> EstadoImportacion(
        [FromQuery] int anio,
        CancellationToken cancellationToken
    )
    {
        var result = await _importacionService.ObtenerEstadoAsync(anio, cancellationToken);

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return Ok(result.Value);
    }

    private ActionResult CrearRespuestaError(IReadOnlyCollection<Error> errors)
    {
        var response = new ApiErrorResponse
        {
            Errors = errors.Select(error => new ApiErrorItem
            {
                Code = error.Code,
                Message = error.Message,
            }).ToArray(),
        };

        if (errors.Any(x => x.Code == DiaInhabilErrors.NoEncontrado.Code))
        {
            return NotFound(response);
        }

        if (errors.Any(x =>
            x.Code == DiaInhabilErrors.FechaDuplicada.Code
            || x.Code == DiaInhabilErrors.VistaPreviaDesactualizada.Code
        ))
        {
            return Conflict(response);
        }

        if (errors.Any(x => x.Code == DiaInhabilErrors.AnioNoDisponible.Code))
        {
            return NotFound(response);
        }

        if (errors.Any(x => x.Code == DiaInhabilErrors.FuenteNoDisponible.Code))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        if (errors.Any(x => x.Code == DiaInhabilErrors.FuenteInvalida.Code))
        {
            return StatusCode(StatusCodes.Status502BadGateway, response);
        }

        return BadRequest(response);
    }
}
