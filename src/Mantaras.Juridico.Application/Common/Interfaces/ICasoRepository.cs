using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface ICasoRepository
{
    Task<Caso?> ObtenerPorIdAsync(long casoId, CancellationToken cancellationToken = default);

    Task AgregarAsync(Caso caso, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Caso>> BuscarAsync(
        string? busqueda,
        FaseCaso? faseInterna,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<int> ContarAsync(
        string? busqueda,
        FaseCaso? faseInterna,
        bool soloActivos,
        CancellationToken cancellationToken = default
    );

    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
