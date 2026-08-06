using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Infrastructure.Persistence;
using Mantaras.Juridico.Infrastructure.Persistence.Repositories;
using Mantaras.Juridico.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mantaras.Juridico.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión DefaultConnection."
            );

        services.AddDbContext<JuridicoDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ICasoRepository, CasoRepository>();
        services.AddScoped<IExpedienteRepository, ExpedienteRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
