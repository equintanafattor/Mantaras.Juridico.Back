using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Casos.Requests;

public sealed class ActualizarCasoRequest
{
    public string Titulo { get; set; } = string.Empty;

    public FaseCaso FaseInterna { get; set; }

    public string? TipoTramite { get; set; }

    public string? Observaciones { get; set; }

    public bool Activo { get; set; } = true;

    public IReadOnlyCollection<CasoClienteRequest> Clientes { get; set; } =
        Array.Empty<CasoClienteRequest>();
}