using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IObservacionRepository
{
    Task<IReadOnlyCollection<Observacion>> ObtenerPorClienteIdAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Observacion>> ObtenerPorCasoIdAsync(
        long casoId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Observacion>> ObtenerPorExpedienteIdAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    );

    Task AgregarAsync(
        Observacion observacion,
        CancellationToken cancellationToken = default
    );

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default
    );
}