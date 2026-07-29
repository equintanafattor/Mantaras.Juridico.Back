using Mantaras.Juridico.Application.Common.Interfaces;

namespace Mantaras.Juridico.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public string Usuario => "sistema";
}
