using FluentValidation;
using Mantaras.Juridico.Application.Features.Casos.Services;
using Mantaras.Juridico.Application.Features.Clientes.Services;
using Mantaras.Juridico.Application.Features.Expedientes.Services;
using Mantaras.Juridico.Application.Features.Observaciones.Services;
using Mantaras.Juridico.Application.Features.Panel.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Mantaras.Juridico.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IClientesService, ClientesService>();
        services.AddScoped<ICasosService, CasosService>();
        services.AddScoped<IExpedientesService, ExpedientesService>();
        services.AddScoped<IPanelService, PanelService>();
        services.AddScoped<IObservacionesService, ObservacionesService>();

        return services;
    }
}
