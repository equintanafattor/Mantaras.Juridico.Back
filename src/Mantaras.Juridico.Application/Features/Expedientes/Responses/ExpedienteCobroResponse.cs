using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Responses;

public sealed class ExpedienteCobroResponse
{
    public long ExpedienteId { get; set; }

    public IReadOnlyCollection<CasoExpedienteResponse> Casos { get; set; } =
        Array.Empty<CasoExpedienteResponse>();

    public TipoExpediente TipoExpediente { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public string? Juzgado { get; set; }

    public string? EstadoLegal { get; set; }

    public bool Activo { get; set; }

    public EnvioCobroResponse EnvioCobro { get; set; } = null!;
}
