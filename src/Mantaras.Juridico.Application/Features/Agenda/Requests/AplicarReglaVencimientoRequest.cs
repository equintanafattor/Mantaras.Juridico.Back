using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Requests;

public sealed class AplicarReglaVencimientoRequest
{
    public DateOnly FechaBase { get; set; }

    public OrigenFechaBaseAgenda OrigenFechaBase { get; set; } =
        OrigenFechaBaseAgenda.Manual;

    public long? EntradaAgendaOrigenId { get; set; }

    public long? CasoOrigenId { get; set; }

    public long? ExpedienteOrigenId { get; set; }

    public long? ObservacionOrigenId { get; set; }

    public string ClaveIdempotencia { get; set; } = string.Empty;

    public IReadOnlyCollection<long> ClienteIds { get; set; } = Array.Empty<long>();

    public IReadOnlyCollection<long> CasoIds { get; set; } = Array.Empty<long>();

    public IReadOnlyCollection<long> ExpedienteIds { get; set; } = Array.Empty<long>();

    public IReadOnlyCollection<long> ResponsableIds { get; set; } = Array.Empty<long>();
}
