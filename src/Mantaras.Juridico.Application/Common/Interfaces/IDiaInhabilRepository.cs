using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IDiaInhabilRepository
{
    Task<DiaInhabil?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> ExisteFechaAsync(
        DateOnly fecha,
        long? idExcluir = null,
        CancellationToken cancellationToken = default
    );

    Task AgregarAsync(DiaInhabil entidad, CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DiaInhabil>> BuscarAsync(
        int? anio,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<int> ContarAsync(
        int? anio,
        bool soloActivos,
        CancellationToken cancellationToken = default
    );
}
