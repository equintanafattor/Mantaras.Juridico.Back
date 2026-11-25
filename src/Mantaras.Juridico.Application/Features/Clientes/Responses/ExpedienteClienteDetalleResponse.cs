namespace Mantaras.Juridico.Application.Features.Clientes.Responses;

public sealed class ExpedienteClienteDetalleResponse
{
    public long ExpedienteId { get; set; }

    public long? ExpedientePadreId { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public long? JuzgadoId { get; set; }
    public string? JuzgadoNombre { get; set; }
    public bool? JuzgadoActivo { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? EstadoLegal { get; set; }

    public bool Activo { get; set; }
}
