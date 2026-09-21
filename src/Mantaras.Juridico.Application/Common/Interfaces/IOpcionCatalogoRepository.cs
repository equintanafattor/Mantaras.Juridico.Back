using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IOpcionCatalogoRepository
{
    Task<OpcionCatalogo?> ObtenerPorIdAsync(
        string tipo,
        long id,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExisteNombreAsync(
        string tipo,
        string nombre,
        long? idExcluir = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExisteActivoAsync(
        string tipo,
        string nombre,
        CancellationToken cancellationToken = default
    );

    Task AgregarAsync(
        OpcionCatalogo entidad,
        CancellationToken cancellationToken = default
    );

    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OpcionCatalogo>> BuscarAsync(
        string tipo,
        string? busqueda,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<int> ContarAsync(
        string tipo,
        string? busqueda,
        bool soloActivos,
        CancellationToken cancellationToken = default
    );
}
