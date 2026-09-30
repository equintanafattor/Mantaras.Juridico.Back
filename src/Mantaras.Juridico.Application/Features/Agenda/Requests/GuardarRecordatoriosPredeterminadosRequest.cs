using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Requests;

public sealed class GuardarRecordatoriosPredeterminadosRequest
{
    public IReadOnlyCollection<RecordatorioPredeterminadoRequest> Recordatorios
    {
        get;
        set;
    } = Array.Empty<RecordatorioPredeterminadoRequest>();
}

public sealed class RecordatorioPredeterminadoRequest
{
    public BaseCalculoRecordatorioAgenda BaseCalculo { get; set; }

    public int MinutosAnticipacion { get; set; }
}
