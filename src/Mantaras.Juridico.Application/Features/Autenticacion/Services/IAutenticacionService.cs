using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Autenticacion.Requests;
using Mantaras.Juridico.Application.Features.Autenticacion.Responses;

namespace Mantaras.Juridico.Application.Features.Autenticacion.Services;

public interface IAutenticacionService
{
    Task<Result<IniciarSesionResponse>> IniciarSesionAsync(
        IniciarSesionRequest request,
        CancellationToken cancellationToken = default
    );
}
