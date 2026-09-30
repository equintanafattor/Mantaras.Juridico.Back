using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Requests;

public sealed class CrearRecordatorioAgendaRequest
{
    public BaseCalculoRecordatorioAgenda BaseCalculo { get; set; } =
        BaseCalculoRecordatorioAgenda.Inicio;

    public int MinutosAnticipacion { get; set; }
}
