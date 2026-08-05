using Mantaras.Juridico.Api.Contracts;
using Mantaras.Juridico.Application.Common.Pagination;
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
            return BadRequest(
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

        var cliente = result.Value!;

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { clienteId = cliente.ClienteId },
            cliente
        );
    }

    [HttpGet("{clienteId:long}")]
    public IActionResult ObtenerPorId(long clienteId)
    {
        return NotFound();
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
}
