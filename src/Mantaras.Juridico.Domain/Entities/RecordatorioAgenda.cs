using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class RecordatorioAgenda : AuditableEntity
{
    public long RecordatorioAgendaId { get; set; }

    public long EntradaAgendaId { get; set; }

    public CanalRecordatorioAgenda Canal { get; set; } = CanalRecordatorioAgenda.Interno;

    public DateTime FechaProgramadaUtc { get; set; }

    public bool Atendido { get; set; }

    public DateTime? FechaAtendidoUtc { get; set; }

    public long? UsuarioAtendioId { get; set; }

    public EntradaAgenda EntradaAgenda { get; set; } = null!;
}
