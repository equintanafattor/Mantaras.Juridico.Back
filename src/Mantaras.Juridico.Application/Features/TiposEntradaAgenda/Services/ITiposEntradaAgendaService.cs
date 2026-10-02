using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Catalogos.Requests;
using Mantaras.Juridico.Application.Features.TiposEntradaAgenda.Requests;
using Mantaras.Juridico.Application.Features.TiposEntradaAgenda.Responses;

namespace Mantaras.Juridico.Application.Features.TiposEntradaAgenda.Services;

public interface ITiposEntradaAgendaService
{
    Task<Result<TipoEntradaAgendaResponse>> CrearAsync(
        GuardarTipoEntradaAgendaRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<TipoEntradaAgendaResponse>> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task<Result<TipoEntradaAgendaResponse>> ActualizarAsync(
        long id,
        GuardarTipoEntradaAgendaRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DarDeBajaAsync(long id, CancellationToken cancellationToken = default);

    Task<Result<bool>> ReactivarAsync(long id, CancellationToken cancellationToken = default);

    Task<Result<PagedResponse<TipoEntradaAgendaResponse>>> BuscarAsync(
        BuscarCatalogosRequest request,
        CancellationToken cancellationToken = default
    );
}
