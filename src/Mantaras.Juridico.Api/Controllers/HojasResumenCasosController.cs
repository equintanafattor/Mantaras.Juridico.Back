using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Casos;
using Mantaras.Juridico.Application.Features.Casos.Requests;
using Mantaras.Juridico.Application.Features.Casos.Responses;
using Mantaras.Juridico.Application.Features.Casos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/casos/{casoId:long}/hoja-resumen")]
public sealed class HojasResumenCasosController : ControllerBase
{
    private readonly IHojaResumenCasoService _service;

    public HojasResumenCasosController(IHojaResumenCasoService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(HojaResumenCasoResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<HojaResumenCasoResponse>> Obtener(
        long casoId,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.ObtenerAsync(
            casoId,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return NotFound(CrearErrorResponse(result.Errors));
        }

        return Ok(result.Value);
    }

    [HttpPut]
    [ProducesResponseType(
        typeof(HojaResumenCasoResponse),
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
    public async Task<ActionResult<HojaResumenCasoResponse>> Guardar(
        long casoId,
        [FromBody] GuardarHojaResumenCasoRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _service.GuardarAsync(
            casoId,
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            var response = CrearErrorResponse(result.Errors);

            if (result.Errors.Any(
                x => x.Code == CasoErrors.NoEncontrado.Code
            ))
            {
                return NotFound(response);
            }

            if (result.Errors.Any(
                x => x.Code == HojaResumenCasoErrors.CreacionConcurrente.Code
            ))
            {
                return Conflict(response);
            }

            return BadRequest(response);
        }

        return Ok(result.Value);
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