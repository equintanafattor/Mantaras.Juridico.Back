using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class RecordatorioPredeterminadoReglaVencimiento : AuditableEntity
{
    public long RecordatorioPredeterminadoReglaVencimientoId { get; set; }

    public long ReglaVencimientoId { get; set; }

    public BaseCalculoRecordatorioAgenda BaseCalculo { get; set; } =
        BaseCalculoRecordatorioAgenda.Vencimiento;

    public int MinutosAnticipacion { get; set; }

    public ReglaVencimiento ReglaVencimiento { get; set; } = null!;
}
