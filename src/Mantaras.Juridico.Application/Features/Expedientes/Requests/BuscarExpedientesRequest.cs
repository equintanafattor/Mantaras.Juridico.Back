using Mantaras.Juridico.Application.Common.Pagination;

namespace Mantaras.Juridico.Application.Features.Expedientes.Requests;

public sealed class BuscarExpedientesRequest : PagedRequest
{
    public long? CasoId { get; set; }

    // Identificador del catálogo de Juzgados. Admite juzgados inactivos.
    public long? JuzgadoId { get; set; }

    public string? Busqueda { get; set; }

    public bool SoloActivos { get; set; } = true;
}