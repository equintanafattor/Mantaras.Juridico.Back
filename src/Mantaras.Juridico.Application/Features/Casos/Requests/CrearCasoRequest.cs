using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Casos.Requests;

public sealed class CrearCasoRequest
{
    public string Titulo { get; set; } = string.Empty;

    public FaseCaso FaseInterna { get; set; }

    public string? TipoTramite { get; set; }

    public string? Observaciones { get; set; }

    public IReadOnlyCollection<CasoClienteRequest> Clientes { get; set; } =
        Array.Empty<CasoClienteRequest>();
}
