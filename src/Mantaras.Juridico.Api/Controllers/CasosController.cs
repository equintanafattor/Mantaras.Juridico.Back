using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Casos;
using Mantaras.Juridico.Application.Features.Casos.Requests;
using Mantaras.Juridico.Application.Features.Casos.Responses;
using Mantaras.Juridico.Application.Features.Casos.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Route("api/casos")]
public sealed class CasosController : ControllerBase
{
    private readonly ICasosService _casosService;

    public CasosController(ICasosService casosService)
    {
        _casosService = casosService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CasoResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    public async Task<ActionResult<CasoResponse>> Crear(
        [FromBody] CrearCasoRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _casosService.CrearAsync(
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return BadRequest(CrearErrorResponse(result.Errors));
        }

        var caso = result.Value!;

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { casoId = caso.CasoId },
            caso
        );
    }

    [HttpGet("{casoId:long}")]
    [ProducesResponseType(
        typeof(CasoResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<CasoResponse>> ObtenerPorId(
        long casoId,
        CancellationToken cancellationToken
    )
    {
        var result = await _casosService.ObtenerPorIdAsync(
            casoId,
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
        typeof(PagedResponse<CasoResponse>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<PagedResponse<CasoResponse>>> Buscar(
        [FromQuery] BuscarCasosRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _casosService.BuscarAsync(
            request,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPut("{casoId:long}")]
    [ProducesResponseType(
        typeof(CasoResponse),
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
    public async Task<ActionResult<CasoResponse>> Actualizar(
        long casoId,
        [FromBody] ActualizarCasoRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _casosService.ActualizarAsync(
            casoId,
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            if (result.Errors.Any(
                x => x.Code == CasoErrors.NoEncontrado.Code
            ))
            {
                return NotFound(CrearErrorResponse(result.Errors));
            }

            return BadRequest(CrearErrorResponse(result.Errors));
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