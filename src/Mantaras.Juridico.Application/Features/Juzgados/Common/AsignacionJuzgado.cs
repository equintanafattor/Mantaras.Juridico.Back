using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Expedientes;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Juzgados.Common;

// Regla compartida por el alta/edición de expedientes y el alta de caso con expediente principal.
public static class AsignacionJuzgado
{
    public static async Task<(Juzgado? Juzgado, Error? Error)> ResolverAsync(
        IJuzgadoRepository repository,
        long? juzgadoId,
        long? juzgadoActualId,
        CancellationToken cancellationToken
    )
    {
        if (!juzgadoId.HasValue)
        {
            return (null, null);
        }

        var juzgado = await repository.ObtenerPorIdAsync(juzgadoId.Value, cancellationToken);

        if (juzgado is null)
        {
            return (null, ExpedienteErrors.JuzgadoNoEncontrado);
        }

        // Se permite conservar un juzgado dado de baja, pero no asignarlo de nuevo.
        if (!juzgado.Activo && juzgadoId != juzgadoActualId)
        {
            return (null, ExpedienteErrors.JuzgadoInactivo);
        }

        return (juzgado, null);
    }
}
