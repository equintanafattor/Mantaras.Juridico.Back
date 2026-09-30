using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class ReglaVencimiento : AuditableEntity
{
    public long ReglaVencimientoId { get; set; }

    public long TipoEntradaAgendaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int CantidadDias { get; set; }

    public TipoComputoPlazo TipoComputo { get; set; }

    public SentidoCalculoPlazo SentidoCalculo { get; set; } =
        SentidoCalculoPlazo.Despues;

    public PrioridadAgenda PrioridadGenerada { get; set; } = PrioridadAgenda.Normal;

    public TimeOnly? HoraSugerida { get; set; }

    public TipoEntradaAgenda TipoEntrada { get; set; } = null!;

    public ICollection<RecordatorioPredeterminadoReglaVencimiento>
        RecordatoriosPredeterminados { get; set; } =
            new List<RecordatorioPredeterminadoReglaVencimiento>();

    public ICollection<AgendaGeneracionRegla> Generaciones { get; set; } =
        new List<AgendaGeneracionRegla>();
}
