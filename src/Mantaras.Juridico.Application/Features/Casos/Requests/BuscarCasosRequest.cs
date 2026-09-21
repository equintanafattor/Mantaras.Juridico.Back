using Mantaras.Juridico.Application.Common.Pagination;

namespace Mantaras.Juridico.Application.Features.Casos.Requests;

public sealed class BuscarCasosRequest : PagedRequest
{
    public string? Busqueda { get; set; }

    public string? FaseInterna { get; set; }

    public bool SoloActivos { get; set; } = true;
}
