using FluentValidation;
using Mantaras.Juridico.Application.Features.Clientes.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Mantaras.Juridico.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IClientesService, ClientesService>();

        return services;
    }
}
