using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IPanelRepository
{
    Task<int> ContarClientesActivosAsync(
        CancellationToken cancellationToken = default
    );

    Task<int> ContarCasosActivosAsync(
        CancellationToken cancellationToken = default
    );

    Task<int> ContarExpedientesActivosAsync(
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Caso>> ObtenerCasosRecientesAsync(
        int cantidad,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Expediente>> ObtenerExpedientesRecientesAsync(
        int cantidad,
        CancellationToken cancellationToken = default
    );
}