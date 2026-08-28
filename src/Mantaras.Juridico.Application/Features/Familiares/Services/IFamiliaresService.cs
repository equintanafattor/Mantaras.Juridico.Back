using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Familiares.Requests;
using Mantaras.Juridico.Application.Features.Familiares.Responses;

namespace Mantaras.Juridico.Application.Features.Familiares.Services;

public interface IFamiliaresService
{
    Task<Result<IReadOnlyCollection<FamiliarResponse>>> ListarAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    );

    Task<Result<FamiliarResponse>> VincularAsync(
        long clienteId,
        VincularFamiliarRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DesvincularAsync(
        long clienteId,
        long familiarId,
        CancellationToken cancellationToken = default
    );
}