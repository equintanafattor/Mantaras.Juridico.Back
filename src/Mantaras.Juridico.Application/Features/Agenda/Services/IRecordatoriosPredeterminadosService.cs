using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;

namespace Mantaras.Juridico.Application.Features.Agenda.Services;

public interface IRecordatoriosPredeterminadosService
{
    Task<Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>>
        ObtenerPorTipoAsync(
            long tipoEntradaAgendaId,
            CancellationToken cancellationToken = default
        );

    Task<Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>>
        GuardarPorTipoAsync(
            long tipoEntradaAgendaId,
            GuardarRecordatoriosPredeterminadosRequest request,
            CancellationToken cancellationToken = default
        );

    Task<Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>>
        ObtenerPorReglaAsync(
            long reglaVencimientoId,
            CancellationToken cancellationToken = default
        );

    Task<Result<IReadOnlyCollection<RecordatorioPredeterminadoResponse>>>
        GuardarPorReglaAsync(
            long reglaVencimientoId,
            GuardarRecordatoriosPredeterminadosRequest request,
            CancellationToken cancellationToken = default
        );
}
