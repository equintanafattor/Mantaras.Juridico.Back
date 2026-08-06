using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(long clienteId, CancellationToken cancellationToken = default);

    Task<bool> ExisteDniAsync(string dni, CancellationToken cancellationToken = default);

    Task<bool> ExisteCuilAsync(string cuil, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Cliente>> ObtenerActivosPorIdsAsync(
        IReadOnlyCollection<long> clienteIds,
        CancellationToken cancellationToken = default
    );

    Task AgregarAsync(Cliente cliente, CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Cliente>> BuscarAsync(
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
