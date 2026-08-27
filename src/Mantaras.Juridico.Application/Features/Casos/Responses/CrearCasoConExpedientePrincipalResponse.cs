namespace Mantaras.Juridico.Application.Features.Casos.Responses;

public sealed class CrearCasoConExpedientePrincipalResponse
{
    public long CasoId { get; init; }

    public long ExpedienteId { get; init; }

    public string TituloCaso { get; init; } = string.Empty;

    public string? NumeroExpedienteAnses { get; init; }

    public long? TipoBeneficioId { get; init; }

    public string? TipoBeneficioNombre { get; init; }

    public bool? TipoBeneficioActivo { get; init; }

    public long? TipoExpedienteAdministrativoId { get; init; }

    public string? TipoExpedienteAdministrativoNombre { get; init; }

    public bool? TipoExpedienteAdministrativoActivo { get; init; }

    public string? NumeroExpediente { get; init; }

    public string Caratula { get; init; } = string.Empty;

    public DateTime FechaCreacion { get; init; }
}
