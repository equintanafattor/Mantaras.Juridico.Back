using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Casos.Responses;

public sealed class ExpedienteCasoDetalleResponse
{
    public long ExpedienteId { get; set; }

    public TipoExpediente TipoExpediente { get; set; }

    public long? ExpedientePadreId { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public string? Juzgado { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? EstadoLegal { get; set; }

    public string? Observaciones { get; set; }

    public bool Activo { get; set; }
}