using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Responses;

public sealed class ExpedienteDetalleResponse
{
    public long ExpedienteId { get; set; }

    public long CasoId { get; set; }

    public string TituloCaso { get; set; } = string.Empty;

    public long? ExpedientePadreId { get; set; }

    public TipoExpediente TipoExpediente { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public string? Juzgado { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? EstadoLegal { get; set; }

    public ExpedienteRelacionadoResponse? ExpedientePadre { get; set; }

    public IReadOnlyCollection<ExpedienteRelacionadoResponse> ExpedientesDerivados { get; set; } =
        Array.Empty<ExpedienteRelacionadoResponse>();

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }
}