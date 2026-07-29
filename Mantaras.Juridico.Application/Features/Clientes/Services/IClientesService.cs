using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Clientes.Requests;
using Mantaras.Juridico.Application.Features.Clientes.Responses;
using Mantaras.Juridico.Application.Common.Pagination;

namespace Mantaras.Juridico.Application.Features.Clientes.Services;

public interface IClientesService
{
    Task<Result<ClienteResponse>> CrearAsync(
        CrearClienteRequest request,
        CancellationToken cancellationToken = default
    );

    Task<PagedResponse<ClienteResponse>> BuscarAsync(
        BuscarClientesRequest request,
        CancellationToken cancellationToken = default
    );
}
