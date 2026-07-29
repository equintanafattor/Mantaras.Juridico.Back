using Mantaras.Juridico.Application.Common.Pagination;

namespace Mantaras.Juridico.Application.Features.Clientes.Requests;

public sealed class BuscarClientesRequest : PagedRequest
{
    public string? Busqueda { get; set; }

    public bool SoloActivos { get; set; } = true;
}
