using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Responses;

public sealed class ReglaVencimientoResponse
{
    public long ReglaVencimientoId { get; set; }

    public long TipoEntradaAgendaId { get; set; }

    public string TipoEntradaNombre { get; set; } = string.Empty;

    public string? TipoEntradaColor { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int CantidadDias { get; set; }

    public TipoComputoPlazo TipoComputo { get; set; }

    public SentidoCalculoPlazo SentidoCalculo { get; set; }

    public PrioridadAgenda PrioridadGenerada { get; set; }

    public TimeOnly? HoraSugerida { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }
}
