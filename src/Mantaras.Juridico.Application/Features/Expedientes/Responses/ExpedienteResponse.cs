using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Responses;

public sealed class ExpedienteResponse
{
    public long ExpedienteId { get; set; }

    public IReadOnlyCollection<CasoExpedienteResponse> Casos { get; set; } =
        Array.Empty<CasoExpedienteResponse>();

    public long? ExpedientePadreId { get; set; }

    public TipoExpediente TipoExpediente { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public string? Juzgado { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? EstadoLegal { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }
}
