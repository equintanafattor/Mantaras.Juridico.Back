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

    public DiasInhabilesController(IDiasInhabilesService service)
    {
        _service = service;
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

        if (errors.Any(x => x.Code == DiaInhabilErrors.FechaDuplicada.Code))
        {
            return Conflict(response);
        }

        return BadRequest(response);
    }
}
