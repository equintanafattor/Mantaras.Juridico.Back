using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Requests;

public sealed class GuardarReglaVencimientoRequest
{
    public long TipoEntradaAgendaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int CantidadDias { get; set; }

    public TipoComputoPlazo TipoComputo { get; set; }

    public SentidoCalculoPlazo SentidoCalculo { get; set; } =
        SentidoCalculoPlazo.Despues;

    public PrioridadAgenda PrioridadGenerada { get; set; } =
        PrioridadAgenda.Normal;

    public TimeOnly? HoraSugerida { get; set; }

    public bool Activo { get; set; } = true;
}
