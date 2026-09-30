using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Responses;

public sealed class RecordatorioPredeterminadoResponse
{
    public long RecordatorioPredeterminadoId { get; set; }

    public BaseCalculoRecordatorioAgenda BaseCalculo { get; set; }

    public int MinutosAnticipacion { get; set; }

    public bool Activo { get; set; }
}
