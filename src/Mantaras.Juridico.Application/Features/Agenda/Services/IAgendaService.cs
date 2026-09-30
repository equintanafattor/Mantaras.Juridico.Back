using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;

namespace Mantaras.Juridico.Application.Features.Agenda.Services;

public interface IAgendaService
{
    Task<Result<EntradaAgendaResponse>> CrearAsync(
        GuardarEntradaAgendaRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<EntradaAgendaResponse>> CrearVencimientoManualAsync(
        GuardarEntradaAgendaRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<EntradaAgendaResponse>> ObtenerPorIdAsync(
        long entradaAgendaId,
        CancellationToken cancellationToken = default
    );

    Task<Result<EntradaAgendaResponse>> ActualizarAsync(
        long entradaAgendaId,
        GuardarEntradaAgendaRequest request,
        CancellationToken cancellationToken = default
    );

    Task<PagedResponse<EntradaAgendaListadoResponse>> BuscarAsync(
        BuscarAgendaRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<EntradaAgendaResponse>> CambiarEstadoAsync(
        long entradaAgendaId,
        CambiarEstadoAgendaRequest request,
        CancellationToken cancellationToken = default
    );
}
