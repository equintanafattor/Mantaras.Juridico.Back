using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class RecordatorioPredeterminadoTipoAgenda : AuditableEntity
{
    public long RecordatorioPredeterminadoTipoAgendaId { get; set; }

    public long TipoEntradaAgendaId { get; set; }

    public BaseCalculoRecordatorioAgenda BaseCalculo { get; set; }

    public int MinutosAnticipacion { get; set; }

    public TipoEntradaAgenda TipoEntradaAgenda { get; set; } = null!;
}
