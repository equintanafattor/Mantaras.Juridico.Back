using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IExpedienteRepository
{
    Task<Expediente?> ObtenerPorIdAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    );

    Task<Expediente?> ObtenerDetallePorIdAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    );

    Task AgregarAsync(
        Expediente expediente,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Expediente>> BuscarAsync(
        long? casoId,
        string? busqueda,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<int> ContarAsync(
        long? casoId,
        string? busqueda,
        bool soloActivos,
        CancellationToken cancellationToken = default
    );

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default
    );

    Task<bool> TieneDerivadosActivosAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistePrincipalAsync(
        long casoId,
        long? expedienteIdExcluir = null,
        CancellationToken cancellationToken = default
    );
}