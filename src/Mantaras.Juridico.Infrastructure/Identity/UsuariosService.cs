using Mantaras.Juridico.Application.Common.Authorization;
using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Usuarios;
using Mantaras.Juridico.Application.Features.Usuarios.Requests;
using Mantaras.Juridico.Application.Features.Usuarios.Responses;
using Mantaras.Juridico.Application.Features.Usuarios.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Infrastructure.Identity;

public sealed class UsuariosService : IUsuariosService
{
    private readonly UserManager<UsuarioIdentity> _userManager;
    private readonly ICurrentUserService _currentUser;

    public UsuariosService(
        UserManager<UsuarioIdentity> userManager,
        ICurrentUserService currentUser
    )
    {
        _userManager = userManager;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<UsuarioResponse>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default
    )
    {
        var usuarios = await _userManager
            .Users.AsNoTracking()
            .OrderByDescending(usuario => usuario.Activo)
            .ThenBy(usuario => usuario.Nombre)
            .ToListAsync(cancellationToken);

        var responses = new List<UsuarioResponse>(usuarios.Count);

        foreach (var usuario in usuarios)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var roles = await _userManager.GetRolesAsync(usuario);

            responses.Add(MapearResponse(usuario, roles));
        }

        return responses;
    }

    public async Task<Result<UsuarioResponse>> CrearAsync(
        CrearUsuarioRequest request,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rol = RolesSistema.Todos.FirstOrDefault(rol =>
            string.Equals(
                rol,
                request.Rol,
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (rol is null)
        {
            return Result<UsuarioResponse>.Failure(
                UsuarioErrors.RolInvalido
            );
        }

        var email = request.Email.Trim();

        var usuarioExistente = await _userManager.FindByEmailAsync(email);

        if (usuarioExistente is not null)
        {
            return Result<UsuarioResponse>.Failure(
                UsuarioErrors.EmailDuplicado
            );
        }

        var usuario = new UsuarioIdentity
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Nombre = request.Nombre.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            LockoutEnabled = true,
        };

        var createResult = await _userManager.CreateAsync(
            usuario,
            request.Password
        );

        if (!createResult.Succeeded)
        {
            if (ContieneEmailDuplicado(createResult.Errors))
            {
                return Result<UsuarioResponse>.Failure(
                    UsuarioErrors.EmailDuplicado
                );
            }

            return Result<UsuarioResponse>.Failure(
                UsuarioErrors.CreacionFallida
            );
        }

        var roleResult = await _userManager.AddToRoleAsync(
            usuario,
            rol
        );

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(usuario);

            return Result<UsuarioResponse>.Failure(
                UsuarioErrors.CreacionFallida
            );
        }

        return Result<UsuarioResponse>.Success(
            MapearResponse(usuario, new[] { rol })
        );
    }

    public async Task<Result<bool>> DarDeBajaAsync(
        long usuarioId,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var usuario = await _userManager.FindByIdAsync(
            usuarioId.ToString()
        );

        if (usuario is null)
        {
            return Result<bool>.Failure(
                UsuarioErrors.NoEncontrado
            );
        }

        if (_currentUser.UsuarioId == usuarioId)
        {
            return Result<bool>.Failure(
                UsuarioErrors.NoPuedeDesactivarse
            );
        }

        if (!usuario.Activo)
        {
            return Result<bool>.Success(true);
        }

        var esAdministrador = await _userManager.IsInRoleAsync(
            usuario,
            RolesSistema.Administrador
        );

        if (esAdministrador)
        {
            var administradores = await _userManager.GetUsersInRoleAsync(
                RolesSistema.Administrador
            );

            var administradoresActivos = administradores.Count(
                administrador => administrador.Activo
            );

            if (administradoresActivos <= 1)
            {
                return Result<bool>.Failure(
                    UsuarioErrors.UltimoAdministrador
                );
            }
        }

        usuario.Activo = false;
        usuario.FechaModificacion = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(usuario);

        if (!updateResult.Succeeded)
        {
            return Result<bool>.Failure(
                UsuarioErrors.ActualizacionFallida
            );
        }

        await _userManager.UpdateSecurityStampAsync(usuario);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RestaurarAsync(
        long usuarioId,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var usuario = await _userManager.FindByIdAsync(
            usuarioId.ToString()
        );

        if (usuario is null)
        {
            return Result<bool>.Failure(
                UsuarioErrors.NoEncontrado
            );
        }

        if (usuario.Activo)
        {
            return Result<bool>.Success(true);
        }

        usuario.Activo = true;
        usuario.FechaModificacion = DateTime.UtcNow;
        usuario.LockoutEnd = null;
        usuario.AccessFailedCount = 0;

        var updateResult = await _userManager.UpdateAsync(usuario);

        if (!updateResult.Succeeded)
        {
            return Result<bool>.Failure(
                UsuarioErrors.ActualizacionFallida
            );
        }

        return Result<bool>.Success(true);
    }

    private static UsuarioResponse MapearResponse(
        UsuarioIdentity usuario,
        IEnumerable<string> roles
    )
    {
        return new UsuarioResponse
        {
            UsuarioId = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email ?? string.Empty,
            Roles = roles.ToArray(),
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            FechaModificacion = usuario.FechaModificacion,
        };
    }

    private static bool ContieneEmailDuplicado(
        IEnumerable<IdentityError> errors
    )
    {
        return errors.Any(error =>
            error.Code is "DuplicateEmail" or "DuplicateUserName"
        );
    }
}