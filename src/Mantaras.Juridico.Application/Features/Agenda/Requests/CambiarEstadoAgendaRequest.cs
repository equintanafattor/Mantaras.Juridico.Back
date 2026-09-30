using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Requests;

public sealed class CambiarEstadoAgendaRequest
{
    public EstadoEntradaAgenda Estado { get; set; }
}
