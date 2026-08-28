using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Clientes.Requests;
using Mantaras.Juridico.Application.Features.Clientes.Responses;

namespace Mantaras.Juridico.Application.Features.Clientes.Services;

public interface IClientesService
{
    Task<Result<ClienteResponse>> CrearAsync(
        CrearClienteRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<ClienteDetalleResponse>> ObtenerPorIdAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    );

    Task<Result<ClienteDetalleResponse>> ActualizarAsync(
        long clienteId,
        ActualizarClienteRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DarDeBajaAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> ReactivarAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    );

    Task<PagedResponse<ClienteResponse>> BuscarAsync(
        BuscarClientesRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<ClaveSeguridadSocialResponse>> ObtenerClaveSeguridadSocialAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    );
}