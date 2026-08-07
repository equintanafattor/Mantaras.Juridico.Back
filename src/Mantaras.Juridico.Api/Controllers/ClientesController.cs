using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Clientes;
using Mantaras.Juridico.Application.Features.Clientes.Requests;
using Mantaras.Juridico.Application.Features.Clientes.Responses;
using Mantaras.Juridico.Application.Features.Clientes.Services;
using Microsoft.AspNetCore.Mvc;

namespace Mantaras.Juridico.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IClientesService _clientesService;

    public ClientesController(IClientesService clientesService)
    {
        _clientesService = clientesService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteResponse>> Crear(
        [FromBody] CrearClienteRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _clientesService.CrearAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(CrearErrorResponse(result.Errors));
        }

        var cliente = result.Value!;

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { clienteId = cliente.ClienteId },
            cliente
        );
    }

    [HttpGet("{clienteId:long}")]
    [ProducesResponseType(typeof(ClienteDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDetalleResponse>> ObtenerPorId(
        long clienteId,
        CancellationToken cancellationToken
    )
    {
        var result = await _clientesService.ObtenerPorIdAsync(clienteId, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(CrearErrorResponse(result.Errors));
        }

        return Ok(result.Value);
    }

    [HttpPut("{clienteId:long}")]
    [ProducesResponseType(typeof(ClienteDetalleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDetalleResponse>> Actualizar(
        long clienteId,
        [FromBody] ActualizarClienteRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _clientesService.ActualizarAsync(clienteId, request, cancellationToken);

        if (result.IsFailure)
        {
            var errorResponse = CrearErrorResponse(result.Errors);

            if (ContieneError(result.Errors, ClienteErrors.NoEncontrado))
            {
                return NotFound(errorResponse);
            }

            return BadRequest(errorResponse);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{clienteId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DarDeBaja(long clienteId, CancellationToken cancellationToken)
    {
        var result = await _clientesService.DarDeBajaAsync(clienteId, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(CrearErrorResponse(result.Errors));
        }

        return NoContent();
    }

    [HttpPatch("{clienteId:long}/reactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivar(
    long clienteId,
    CancellationToken cancellationToken
)
    {
        var result = await _clientesService.ReactivarAsync(
            clienteId,
            cancellationToken
        );

        if (result.IsFailure)
        {
            return NotFound(CrearErrorResponse(result.Errors));
        }

        return NoContent();
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ClienteResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ClienteResponse>>> Buscar(
        [FromQuery] BuscarClientesRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await _clientesService.BuscarAsync(request, cancellationToken);

        return Ok(response);
    }

    private static ApiErrorResponse CrearErrorResponse(IReadOnlyCollection<Error> errors)
    {
        return new ApiErrorResponse
        {
            Errors = errors
                .Select(error => new ApiErrorItem { Code = error.Code, Message = error.Message })
                .ToArray(),
        };
    }

    private static bool ContieneError(IReadOnlyCollection<Error> errors, Error expectedError)
    {
        return errors.Any(error => error.Code == expectedError.Code);
    }
}
