namespace Mantaras.Juridico.Application.Features.Expedientes.Requests;

public sealed class ActualizarExpedienteRequest
{
    public long? ExpedientePadreId { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public string? Juzgado { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? EstadoLegal { get; set; }
}