using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Catalogos.Requests;
using Mantaras.Juridico.Application.Features.OpcionesCatalogo;
using Mantaras.Juridico.Application.Features.OpcionesCatalogo.Requests;
using Mantaras.Juridico.Application.Features.OpcionesCatalogo.Responses;
using Mantaras.Juridico.Application.Features.OpcionesCatalogo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/opciones-catalogo/{tipo}")]
public sealed class OpcionesCatalogoController : ControllerBase
{
    private readonly IOpcionesCatalogoService _service;

    public OpcionesCatalogoController(IOpcionesCatalogoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<OpcionCatalogoResponse>>> Buscar(
        string tipo,
        [FromQuery] BuscarCatalogosRequest request,
        CancellationToken cancellationToken
    ) => Resolver(await _service.BuscarAsync(tipo, request, cancellationToken));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<OpcionCatalogoResponse>> ObtenerPorId(
        string tipo,
        long id,
        CancellationToken cancellationToken
    ) => Resolver(await _service.ObtenerPorIdAsync(tipo, id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<OpcionCatalogoResponse>> Crear(
        string tipo,
        [FromBody] GuardarOpcionCatalogoRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.CrearAsync(tipo, request, cancellationToken);
        if (result.IsFailure) return Resolver(result);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<OpcionCatalogoResponse>> Actualizar(
        string tipo,
        long id,
        [FromBody] GuardarOpcionCatalogoRequest request,
        CancellationToken cancellationToken
    ) => Resolver(await _service.ActualizarAsync(tipo, id, request, cancellationToken));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DarDeBaja(
        string tipo,
        long id,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.CambiarEstadoAsync(tipo, id, false, cancellationToken);
        return result.IsFailure ? ResolverError(result.Errors) : NoContent();
    }

    [HttpPatch("{id:long}/reactivar")]
    public async Task<IActionResult> Reactivar(
        string tipo,
        long id,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.CambiarEstadoAsync(tipo, id, true, cancellationToken);
        return result.IsFailure ? ResolverError(result.Errors) : NoContent();
    }

    private ActionResult<T> Resolver<T>(Result<T> result)
    {
        return result.IsFailure ? ResolverError(result.Errors) : Ok(result.Value);
    }

    private ActionResult ResolverError(IReadOnlyCollection<Error> errors)
    {
        var response = new ApiErrorResponse
        {
            Errors = errors.Select(x => new ApiErrorItem
            {
                Code = x.Code,
                Message = x.Message,
            }).ToArray(),
        };

        if (errors.Any(x => x.Code == OpcionCatalogoErrors.NoEncontrado.Code))
            return NotFound(response);

        if (errors.Any(x => x.Code == OpcionCatalogoErrors.NombreDuplicado.Code))
            return Conflict(response);

        return BadRequest(response);
    }
}
