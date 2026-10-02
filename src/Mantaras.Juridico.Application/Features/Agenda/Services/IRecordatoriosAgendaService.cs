using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;

namespace Mantaras.Juridico.Application.Features.Agenda.Services;

public interface IRecordatoriosAgendaService
{
    Task<Result<RecordatorioAgendaResponse>> CrearAsync(
        long entradaAgendaId,
        CrearRecordatorioAgendaRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<RecordatorioAgendaResponse>> ReprogramarAsync(
        long recordatorioAgendaId,
        CrearRecordatorioAgendaRequest request,
        CancellationToken cancellationToken = default
    );
    Task<Result<bool>> QuitarAsync(long recordatorioAgendaId, CancellationToken cancellationToken = default);

    Task<PagedResponse<RecordatorioAgendaResponse>> BuscarAsync(
        BuscarRecordatoriosAgendaRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<RecordatorioAgendaResponse>> AtenderAsync(
        long recordatorioAgendaId,
        CancellationToken cancellationToken = default
    );
}
