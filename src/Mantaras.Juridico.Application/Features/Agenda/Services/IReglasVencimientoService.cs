using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Agenda.Services;

public interface IReglasVencimientoService
{
    Task<IReadOnlyCollection<ReglaVencimientoResponse>> ObtenerTodasAsync(
        bool soloActivas,
        CancellationToken cancellationToken = default
    );

    Task<Result<ReglaVencimientoResponse>> ObtenerPorIdAsync(
        long reglaVencimientoId,
        CancellationToken cancellationToken = default
    );

    Task<Result<ReglaVencimientoResponse>> CrearAsync(
        GuardarReglaVencimientoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<ReglaVencimientoResponse>> ActualizarAsync(
        long reglaVencimientoId,
        GuardarReglaVencimientoRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<AplicacionReglaVencimientoResponse>> AplicarAsync(
        long reglaVencimientoId,
        AplicarReglaVencimientoRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Prepara la generación de la regla identificada por su código sin confirmar
    /// los cambios: el llamador guarda la generación junto con su propio registro.
    /// Si la clave de idempotencia ya fue aplicada, devuelve la generación existente.
    /// </summary>
    Task<Result<AgendaGeneracionRegla>> PrepararAplicacionPorCodigoAsync(
        string codigoRegla,
        AplicarReglaVencimientoRequest request,
        CancellationToken cancellationToken = default
    );
}
