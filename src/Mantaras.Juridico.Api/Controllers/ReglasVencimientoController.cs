using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;
using Mantaras.Juridico.Application.Features.Agenda.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Route("api/reglas-vencimiento")]
public sealed class ReglasVencimientoController : ControllerBase
{
    private readonly IReglasVencimientoService _reglasService;

    public ReglasVencimientoController(
        IReglasVencimientoService reglasService
    )
    {
        _reglasService = reglasService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<ReglaVencimientoResponse>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<IReadOnlyCollection<ReglaVencimientoResponse>>>
        ObtenerTodas(
            [FromQuery] bool soloActivas = true,
            CancellationToken cancellationToken = default
        )
    {
        var reglas = await _reglasService.ObtenerTodasAsync(
            soloActivas,
            cancellationToken
        );

        return Ok(reglas);
    }

    [HttpGet("{reglaVencimientoId:long}")]
    [ProducesResponseType(
        typeof(ReglaVencimientoResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<ReglaVencimientoResponse>> ObtenerPorId(
        long reglaVencimientoId,
        CancellationToken cancellationToken
    )
    {
        var result = await _reglasService.ObtenerPorIdAsync(
            reglaVencimientoId,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return NotFound(CrearErrorResponse(result.Errors));
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ReglaVencimientoResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    public async Task<ActionResult<ReglaVencimientoResponse>> Crear(
        [FromBody] GuardarReglaVencimientoRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _reglasService.CrearAsync(
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return BadRequest(CrearErrorResponse(result.Errors));
        }

        var regla = result.Value!;

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { reglaVencimientoId = regla.ReglaVencimientoId },
            regla
        );
    }

    [HttpPut("{reglaVencimientoId:long}")]
    [ProducesResponseType(
        typeof(ReglaVencimientoResponse),
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
    public async Task<ActionResult<ReglaVencimientoResponse>> Actualizar(
        long reglaVencimientoId,
        [FromBody] GuardarReglaVencimientoRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _reglasService.ActualizarAsync(
            reglaVencimientoId,
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            var errorResponse = CrearErrorResponse(result.Errors);

            return ContieneError(result.Errors, AgendaErrors.ReglaNoEncontrada)
                ? NotFound(errorResponse)
                : BadRequest(errorResponse);
        }

        return Ok(result.Value);
    }

    [HttpPost("{reglaVencimientoId:long}/aplicar")]
    [ProducesResponseType(
        typeof(AplicacionReglaVencimientoResponse),
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
    public async Task<ActionResult<AplicacionReglaVencimientoResponse>> Aplicar(
        long reglaVencimientoId,
        [FromBody] AplicarReglaVencimientoRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _reglasService.AplicarAsync(
            reglaVencimientoId,
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            var errorResponse = CrearErrorResponse(result.Errors);

            return ContieneError(result.Errors, AgendaErrors.ReglaNoEncontrada)
                ? NotFound(errorResponse)
                : BadRequest(errorResponse);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
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
