namespace Mantaras.Juridico.Application.Features.Casos.Responses;

public sealed class CrearCasoConExpedientePrincipalResponse
{
    public long CasoId { get; init; }

    public long ExpedienteId { get; init; }

    public string TituloCaso { get; init; } = string.Empty;

    public string? NumeroExpediente { get; init; }

    public string Caratula { get; init; } = string.Empty;

    public DateTime FechaCreacion { get; init; }
}