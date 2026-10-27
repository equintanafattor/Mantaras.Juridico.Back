using Mantaras.Juridico.Application.Common.Pagination;

namespace Mantaras.Juridico.Application.Features.Expedientes.Requests;

public sealed class BuscarExpedientesCobroRequest : PagedRequest
{
    public string? Busqueda { get; set; }

    public bool SoloActivos { get; set; } = true;

    public bool? SeguimientoVencido { get; set; }
}
