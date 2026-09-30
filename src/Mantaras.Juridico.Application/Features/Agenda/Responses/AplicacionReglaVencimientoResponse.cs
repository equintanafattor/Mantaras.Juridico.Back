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

    public EntradaAgendaResponse Entrada { get; set; } = null!;
}
