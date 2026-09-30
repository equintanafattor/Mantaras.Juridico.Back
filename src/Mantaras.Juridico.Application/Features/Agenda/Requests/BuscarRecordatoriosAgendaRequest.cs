using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Requests;

public sealed class BuscarRecordatoriosAgendaRequest
{
    public long? EntradaAgendaId { get; set; }

    public EstadoRecordatorioAgenda? Estado { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
