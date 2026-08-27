using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface ITipoBeneficioRepository
{
    Task<TipoBeneficio?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreAsync(
        string nombreNormalizado,
        long? idExcluir = null,
        CancellationToken cancellationToken = default
    );

    Task AgregarAsync(TipoBeneficio entidad, CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TipoBeneficio>> BuscarAsync(
        string? busqueda,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<int> ContarAsync(
        string? busqueda,
        bool soloActivos,
        CancellationToken cancellationToken = default
    );
}
