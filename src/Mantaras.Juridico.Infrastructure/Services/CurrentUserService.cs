using System.Security.Claims;
using Mantaras.Juridico.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Mantaras.Juridico.Infrastructure.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UsuarioId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            return long.TryParse(value, out var usuarioId) ? usuarioId : null;
        }
    }

    public string Usuario
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            return user?.FindFirstValue(ClaimTypes.Name)
                ?? user?.FindFirstValue(ClaimTypes.Email)
                ?? user?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "sistema";
        }
    }
}
