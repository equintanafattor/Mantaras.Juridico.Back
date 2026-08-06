using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Casos.Requests;

public sealed class BuscarCasosRequest : PagedRequest
{
    public string? Busqueda { get; set; }

    public FaseCaso? FaseInterna { get; set; }

    public bool SoloActivos { get; set; } = true;
}