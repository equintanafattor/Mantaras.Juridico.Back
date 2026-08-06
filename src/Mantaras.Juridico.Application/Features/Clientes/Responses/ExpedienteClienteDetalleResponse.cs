namespace Mantaras.Juridico.Application.Features.Clientes.Responses;

public sealed class ExpedienteClienteDetalleResponse
{
    public long ExpedienteId { get; set; }

    public long? ExpedientePadreId { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public string? Juzgado { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? EstadoLegal { get; set; }

    public bool Activo { get; set; }
}
