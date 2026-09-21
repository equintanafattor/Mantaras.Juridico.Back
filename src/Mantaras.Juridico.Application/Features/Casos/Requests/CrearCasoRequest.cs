namespace Mantaras.Juridico.Application.Features.Casos.Requests;

public sealed class CrearCasoRequest
{
    public string Titulo { get; set; } = string.Empty;

    public string FaseInterna { get; set; } = string.Empty;

    public string? TipoTramite { get; set; }

    public string? NumeroExpedienteAnses { get; set; }

    public string? NumeroBeneficio { get; set; }

    public long? TipoBeneficioId { get; set; }

    public long? TipoExpedienteAdministrativoId { get; set; }

    public IReadOnlyCollection<CasoClienteRequest> Clientes { get; set; } =
        Array.Empty<CasoClienteRequest>();
}
