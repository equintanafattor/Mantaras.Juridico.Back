using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public sealed class PanelRepository : IPanelRepository
{
    private readonly JuridicoDbContext _dbContext;

    public PanelRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> ContarClientesActivosAsync(
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.Clientes.CountAsync(
            x => x.Activo,
            cancellationToken
        );
    }

    public Task<int> ContarCasosActivosAsync(
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.Casos.CountAsync(
            x => x.Activo,
            cancellationToken
        );
    }

    public Task<int> ContarExpedientesActivosAsync(
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.Expedientes.CountAsync(
            x => x.Activo,
            cancellationToken
        );
    }

    public async Task<IReadOnlyCollection<Caso>> ObtenerCasosRecientesAsync(
        int cantidad,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext
            .Casos.AsNoTracking()
            .Where(x => x.Activo)
            .OrderByDescending(
                x => x.FechaModificacion ?? x.FechaCreacion
            )
            .Take(cantidad)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Expediente>>
        ObtenerExpedientesRecientesAsync(
            int cantidad,
            CancellationToken cancellationToken = default
        )
    {
        return await _dbContext
            .Expedientes.AsNoTracking()
            .Include(x => x.Casos)
                .ThenInclude(x => x.Caso)
            .Where(x => x.Activo && x.Casos.Any(relacion => relacion.Caso.Activo))
            .OrderByDescending(
                x => x.FechaModificacion ?? x.FechaCreacion
            )
            .Take(cantidad)
            .ToListAsync(cancellationToken);
    }

    public async Task<PanelAgendaData> ObtenerResumenAgendaAsync(
        DateOnly hoy,
        int cantidadProximos,
        CancellationToken cancellationToken = default
    )
    {
        var entradasActivas = _dbContext
            .EntradasAgenda.AsNoTracking()
            .Where(x =>
                x.Activo
                && x.Estado != EstadoEntradaAgenda.Completada
                && x.Estado != EstadoEntradaAgenda.Cancelada
            );

        var metricas = await entradasActivas
            .GroupBy(_ => 1)
            .Select(grupo => new
            {
                Hoy = grupo.Count(x =>
                    (x.FechaVencimiento ?? x.FechaInicio) == hoy
                ),
                Proximos = grupo.Count(x =>
                    (x.FechaVencimiento ?? x.FechaInicio) > hoy
                ),
                Vencidos = grupo.Count(x =>
                    (x.FechaVencimiento ?? x.FechaInicio) < hoy
                ),
            })
            .SingleOrDefaultAsync(cancellationToken);

        var elementosProximos = await entradasActivas
            .Where(x => (x.FechaVencimiento ?? x.FechaInicio) >= hoy)
            .OrderBy(x => x.FechaVencimiento ?? x.FechaInicio)
            .ThenBy(x => x.HoraVencimiento ?? x.HoraInicio)
            .ThenByDescending(x => x.Prioridad)
            .ThenBy(x => x.EntradaAgendaId)
            .Take(cantidadProximos)
            .Select(x => new PanelAgendaItemData
            {
                EntradaAgendaId = x.EntradaAgendaId,
                Titulo = x.Titulo,
                TipoEntradaNombre = x.TipoEntrada.Nombre,
                TipoEntradaColor = x.TipoEntrada.Color,
                Estado = x.Estado,
                Prioridad = x.Prioridad,
                FechaReferencia = x.FechaVencimiento ?? x.FechaInicio,
                HoraReferencia = x.FechaVencimiento.HasValue
                    ? x.HoraVencimiento
                    : x.HoraInicio,
                Clientes = x.Clientes
                    .OrderBy(relacion => relacion.Cliente.Apellido)
                    .ThenBy(relacion => relacion.Cliente.Nombre)
                    .Select(relacion => new PanelAgendaContextoData
                    {
                        Id = relacion.ClienteId,
                        Nombre = relacion.Cliente.Apellido
                            + ", "
                            + relacion.Cliente.Nombre,
                    })
                    .ToArray(),
                Casos = x.Casos
                    .OrderBy(relacion => relacion.Caso.Titulo)
                    .Select(relacion => new PanelAgendaContextoData
                    {
                        Id = relacion.CasoId,
                        Nombre = relacion.Caso.Titulo,
                    })
                    .ToArray(),
                Expedientes = x.Expedientes
                    .OrderBy(relacion => relacion.Expediente.Caratula)
                    .Select(relacion => new PanelAgendaContextoData
                    {
                        Id = relacion.ExpedienteId,
                        Nombre = relacion.Expediente.Caratula,
                    })
                    .ToArray(),
            })
            .ToListAsync(cancellationToken);

        return new PanelAgendaData
        {
            Hoy = metricas?.Hoy ?? 0,
            Proximos = metricas?.Proximos ?? 0,
            Vencidos = metricas?.Vencidos ?? 0,
            ElementosProximos = elementosProximos,
        };
    }
}
