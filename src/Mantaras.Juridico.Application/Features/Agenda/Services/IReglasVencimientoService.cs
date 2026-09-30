using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Agenda.Requests;
using Mantaras.Juridico.Application.Features.Agenda.Responses;

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
}
