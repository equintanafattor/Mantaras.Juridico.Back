using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Authorization;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Usuarios;
using Mantaras.Juridico.Application.Features.Usuarios.Requests;
using Mantaras.Juridico.Application.Features.Usuarios.Responses;
using Mantaras.Juridico.Application.Features.Usuarios.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = RolesSistema.Administrador)]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _usuariosService;

    public UsuariosController(IUsuariosService usuariosService)
    {
        _usuariosService = usuariosService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<UsuarioResponse>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<IReadOnlyCollection<UsuarioResponse>>>
        ObtenerTodos(
            CancellationToken cancellationToken
        )
    {
        var usuarios = await _usuariosService.ObtenerTodosAsync(
            cancellationToken
        );

        return Ok(usuarios);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(UsuarioResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status409Conflict
    )]
    public async Task<ActionResult<UsuarioResponse>> Crear(
        [FromBody] CrearUsuarioRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _usuariosService.CrearAsync(
            request,
            cancellationToken
        );

        if (result.IsFailure)
        {
            var errorResponse = CrearErrorResponse(result.Errors);

            if (ContieneError(
                result.Errors,
                UsuarioErrors.EmailDuplicado
            ))
            {
                return Conflict(errorResponse);
            }

            return BadRequest(errorResponse);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.Value
        );
    }

    [HttpDelete("{usuarioId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status409Conflict
    )]
    public async Task<IActionResult> DarDeBaja(
        long usuarioId,
        CancellationToken cancellationToken
    )
    {
        var result = await _usuariosService.DarDeBajaAsync(
            usuarioId,
            cancellationToken
        );

        if (result.IsFailure)
        {
            var errorResponse = CrearErrorResponse(result.Errors);

            if (ContieneError(
                result.Errors,
                UsuarioErrors.NoEncontrado
            ))
            {
                return NotFound(errorResponse);
            }

            return Conflict(errorResponse);
        }

        return NoContent();
    }

    [HttpPatch("{usuarioId:long}/restaurar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest
    )]
    public async Task<IActionResult> Restaurar(
        long usuarioId,
        CancellationToken cancellationToken
    )
    {
        var result = await _usuariosService.RestaurarAsync(
            usuarioId,
            cancellationToken
        );

        if (result.IsFailure)
        {
            var errorResponse = CrearErrorResponse(result.Errors);

            if (ContieneError(
                result.Errors,
                UsuarioErrors.NoEncontrado
            ))
            {
                return NotFound(errorResponse);
            }

            return BadRequest(errorResponse);
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
        return errors.Any(
            error => error.Code == expectedError.Code
        );
    }
}