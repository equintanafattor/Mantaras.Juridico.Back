using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Clientes;
using Mantaras.Juridico.Application.Features.Familiares;
using Mantaras.Juridico.Application.Features.Familiares.Requests;
using Mantaras.Juridico.Application.Features.Familiares.Responses;
using Mantaras.Juridico.Application.Features.Familiares.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/clientes/{clienteId:long}/familiares")]
public sealed class FamiliaresController : ControllerBase
{
    private readonly IFamiliaresService _familiaresService;

    public FamiliaresController(IFamiliaresService familiaresService)
    {
        _familiaresService = familiaresService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(FamiliarResponse[]),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<IReadOnlyCollection<FamiliarResponse>>> Listar(
        long clienteId,
        CancellationToken cancellationToken
    )
    {
        var result = await _familiaresService.ListarAsync(
            clienteId,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(FamiliarResponse),
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
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status409Conflict
    )]
    public async Task<ActionResult<FamiliarResponse>> Vincular(
        long clienteId,
        [FromBody] VincularFamiliarRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _familiaresService.VincularAsync(
            clienteId,
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{familiarId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status409Conflict
    )]
    public async Task<IActionResult> Desvincular(
        long clienteId,
        long familiarId,
        CancellationToken cancellationToken
    )
    {
        var result = await _familiaresService.DesvincularAsync(
            clienteId,
            familiarId,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return CrearRespuestaError(result.Errors);
        }

        return NoContent();
    }

    private ActionResult CrearRespuestaError(
        IReadOnlyCollection<Error> errors
    )
    {
        var response = new ApiErrorResponse
        {
            Errors = errors
                .Select(error => new ApiErrorItem
                {
                    Code = error.Code,
                    Message = error.Message,
                })
                .ToArray(),
        };

        if (errors.Any(error =>
            error.Code == ClienteErrors.NoEncontrado.Code
            || error.Code == FamiliaresErrors.FamiliarNoEncontrado.Code
            || error.Code == FamiliaresErrors.RelacionNoEncontrada.Code
        ))
        {
            return NotFound(response);
        }

        if (errors.Any(error =>
            error.Code == FamiliaresErrors.ParentescoDiferente.Code
            || error.Code == FamiliaresErrors.ConflictoGuardado.Code
        ))
        {
            return Conflict(response);
        }

        return BadRequest(response);
    }
}