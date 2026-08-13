using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Usuarios.Requests;
using Mantaras.Juridico.Application.Features.Usuarios.Responses;

namespace Mantaras.Juridico.Application.Features.Usuarios.Services;

public interface IUsuariosService
{
    Task<IReadOnlyCollection<UsuarioResponse>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default
    );

    Task<Result<UsuarioResponse>> CrearAsync(
        CrearUsuarioRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> DarDeBajaAsync(
        long usuarioId,
        CancellationToken cancellationToken = default
    );

    Task<Result<bool>> RestaurarAsync(
        long usuarioId,
        CancellationToken cancellationToken = default
    );
}
