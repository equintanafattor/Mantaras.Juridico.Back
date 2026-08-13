using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.Autenticacion;
using Mantaras.Juridico.Application.Features.Autenticacion.Requests;
using Mantaras.Juridico.Application.Features.Autenticacion.Responses;
using Mantaras.Juridico.Application.Features.Autenticacion.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Mantaras.Juridico.Infrastructure.Identity;

public sealed class AutenticacionService : IAutenticacionService
{
    private readonly UserManager<UsuarioIdentity> _userManager;
    private readonly JwtOptions _jwtOptions;

    public AutenticacionService(
        UserManager<UsuarioIdentity> userManager,
        IOptions<JwtOptions> jwtOptions
    )
    {
        _userManager = userManager;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<IniciarSesionResponse>> IniciarSesionAsync(
        IniciarSesionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var email = request.Email.Trim();

        var usuario = await _userManager.FindByEmailAsync(email);

        if (usuario is null || !usuario.Activo || await _userManager.IsLockedOutAsync(usuario))
        {
            return Result<IniciarSesionResponse>.Failure(AutenticacionErrors.CredencialesInvalidas);
        }

        var passwordValida = await _userManager.CheckPasswordAsync(usuario, request.Password);

        if (!passwordValida)
        {
            await _userManager.AccessFailedAsync(usuario);

            return Result<IniciarSesionResponse>.Failure(AutenticacionErrors.CredencialesInvalidas);
        }

        await _userManager.ResetAccessFailedCountAsync(usuario);

        var roles = await _userManager.GetRolesAsync(usuario);
        var securityStamp = await _userManager.GetSecurityStampAsync(usuario);

        if (string.IsNullOrWhiteSpace(securityStamp))
        {
            return Result<IniciarSesionResponse>.Failure(
                AutenticacionErrors.CredencialesInvalidas
            );
        }

        var expiraEnUtc = DateTime.UtcNow.AddMinutes(
            _jwtOptions.ExpirationMinutes
        );

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(JwtRegisteredClaimNames.Email, usuario.Email!),
            new(ClaimTypes.Email, usuario.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtClaimTypes.SecurityStamp, securityStamp),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiraEnUtc,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

        return Result<IniciarSesionResponse>.Success(
            new IniciarSesionResponse
            {
                AccessToken = accessToken,
                ExpiraEnUtc = expiraEnUtc,
                Usuario = new UsuarioAutenticadoResponse
                {
                    UsuarioId = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email!,
                    Roles = roles.ToArray(),
                },
            }
        );
    }
}
