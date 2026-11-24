using Mantaras.Juridico.Application.Features.DiasInhabiles.Responses;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Responses;

public sealed class AplicacionReglaVencimientoResponse
{
    public long AgendaGeneracionReglaId { get; set; }

    public long ReglaVencimientoId { get; set; }

    public string ReglaNombre { get; set; } = string.Empty;

    public DateOnly FechaBase { get; set; }

    public DateOnly FechaCalculada { get; set; }

    public OrigenFechaBaseAgenda OrigenFechaBase { get; set; }

    public long? EntradaAgendaOrigenId { get; set; }

    public long? CasoOrigenId { get; set; }

    public long? ExpedienteOrigenId { get; set; }

    public long? ObservacionOrigenId { get; set; }

    public string ClaveIdempotencia { get; set; } = string.Empty;

    // Años del cómputo en días hábiles sin calendario de feriados importado al
    // calcular. Vacío si no aplica o si el cómputo es en días corridos.
    public IReadOnlyCollection<AdvertenciaCalendarioResponse> Advertencias { get; set; } =
        Array.Empty<AdvertenciaCalendarioResponse>();

    public EntradaAgendaResponse Entrada { get; set; } = null!;
}
