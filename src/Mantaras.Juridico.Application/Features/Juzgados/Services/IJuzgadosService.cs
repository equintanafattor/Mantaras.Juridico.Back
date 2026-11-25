using Mantaras.Juridico.Application.Common.Pagination;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Catalogos.Requests;
using Mantaras.Juridico.Application.Features.Juzgados.Requests;
using Mantaras.Juridico.Application.Features.Juzgados.Responses;

namespace Mantaras.Juridico.Application.Features.Juzgados.Services;

public interface IJuzgadosService
{
    Task<Result<JuzgadoResponse>> CrearAsync(
        GuardarJuzgadoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<JuzgadoResponse>> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    );

    Task<Result<JuzgadoResponse>> ActualizarAsync(
        long id,
        GuardarJuzgadoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DarDeBajaAsync(long id, CancellationToken cancellationToken = default);

    Task<Result<bool>> ReactivarAsync(long id, CancellationToken cancellationToken = default);

    Task<Result<PagedResponse<JuzgadoResponse>>> BuscarAsync(
        BuscarCatalogosRequest request,
        CancellationToken cancellationToken = default
    );
}
