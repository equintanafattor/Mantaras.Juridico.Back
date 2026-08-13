using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Features.Autenticacion.Requests;
using Mantaras.Juridico.Application.Features.Autenticacion.Responses;
using Mantaras.Juridico.Application.Features.Autenticacion.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAutenticacionService _autenticacionService;

    public AuthController(IAutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(IniciarSesionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IniciarSesionResponse>> IniciarSesion(
        [FromBody] IniciarSesionRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _autenticacionService.IniciarSesionAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(
                new ApiErrorResponse
                {
                    Errors = result
                        .Errors.Select(error => new ApiErrorItem
                        {
                            Code = error.Code,
                            Message = error.Message,
                        })
                        .ToArray(),
                }
            );
        }

        return Ok(result.Value);
    }
}
