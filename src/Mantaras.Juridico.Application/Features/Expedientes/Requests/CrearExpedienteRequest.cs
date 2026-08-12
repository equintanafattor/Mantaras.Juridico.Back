using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Requests;

public sealed class CrearExpedienteRequest
{
    public long CasoId { get; set; }

    public long? ExpedientePadreId { get; set; }
    
    public TipoExpediente TipoExpediente { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public string? Juzgado { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? EstadoLegal { get; set; }
}