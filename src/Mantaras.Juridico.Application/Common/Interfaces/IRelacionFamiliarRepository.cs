using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IRelacionFamiliarRepository
{
    Task<IReadOnlyCollection<RelacionFamiliar>> ListarPorClienteAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    );

    Task<RelacionFamiliar?> ObtenerPorParejaAsync(
        long clienteAId,
        long clienteBId,
        CancellationToken cancellationToken = default
    );

    Task AgregarAsync(
        RelacionFamiliar relacion,
        CancellationToken cancellationToken = default
    );

    Task<bool> IntentarGuardarCambiosAsync(
        CancellationToken cancellationToken = default
    );
}