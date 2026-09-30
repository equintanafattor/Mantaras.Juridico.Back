using FluentValidation;
using Mantaras.Juridico.Application.Features.Agenda.Services;
using Mantaras.Juridico.Application.Features.Casos.Services;
using Mantaras.Juridico.Application.Features.Clientes.Services;
using Mantaras.Juridico.Application.Features.Expedientes.Services;
using Mantaras.Juridico.Application.Features.Familiares.Services;
using Mantaras.Juridico.Application.Features.Observaciones.Services;
using Mantaras.Juridico.Application.Features.OpcionesCatalogo.Services;
using Mantaras.Juridico.Application.Features.Panel.Services;
using Mantaras.Juridico.Application.Features.TiposBeneficio.Services;
using Mantaras.Juridico.Application.Features.TiposExpedienteAdministrativo.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Mantaras.Juridico.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAgendaService, AgendaService>();
        services.AddScoped<IReglasVencimientoService, ReglasVencimientoService>();
        services.AddScoped<
            IRecordatoriosAgendaService,
            RecordatoriosAgendaService
        >();
        services.AddScoped<
            IRecordatoriosPredeterminadosService,
            RecordatoriosPredeterminadosService
        >();
        services.AddScoped<IClientesService, ClientesService>();
        services.AddScoped<ICasosService, CasosService>();
        services.AddScoped<IHojaResumenCasoService, HojaResumenCasoService>();
        services.AddScoped<IExpedientesService, ExpedientesService>();
        services.AddScoped<IPanelService, PanelService>();
        services.AddScoped<IObservacionesService, ObservacionesService>();
        services.AddScoped<IOpcionesCatalogoService, OpcionesCatalogoService>();

        services.AddScoped<ITiposBeneficioService, TiposBeneficioService>();
        services.AddScoped<ITiposExpedienteAdministrativoService, TiposExpedienteAdministrativoService>();

        services.AddScoped<IFamiliaresService, FamiliaresService>();

        return services;
    }
}
