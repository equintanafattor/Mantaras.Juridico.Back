using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Casos.Responses;

public sealed class CasoResponse
{
    public long CasoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public FaseCaso FaseInterna { get; set; }

    public string? TipoTramite { get; set; }

    public string? Observaciones { get; set; }

    public IReadOnlyCollection<CasoClienteResponse> Clientes { get; set; } =
        Array.Empty<CasoClienteResponse>();

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }
}
