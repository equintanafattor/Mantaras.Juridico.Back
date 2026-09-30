using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Responses;

public sealed class RecordatorioAgendaResponse
{
    public long RecordatorioAgendaId { get; set; }

    public long EntradaAgendaId { get; set; }

    public string EntradaTitulo { get; set; } = string.Empty;

    public string TipoEntradaNombre { get; set; } = string.Empty;

    public string? TipoEntradaColor { get; set; }

    public CanalRecordatorioAgenda Canal { get; set; }

    public DateTime FechaProgramadaUtc { get; set; }

    public EstadoRecordatorioAgenda Estado { get; set; }

    public bool Atendido { get; set; }

    public DateTime? FechaAtendidoUtc { get; set; }

    public long? UsuarioAtendioId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Activo { get; set; }
}
