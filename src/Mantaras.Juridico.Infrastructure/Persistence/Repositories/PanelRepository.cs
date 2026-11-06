using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.Agenda;
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
        // Criterio de CriteriosAgenda. Las cuatro métricas separan todas las entradas abiertas:
        // - Vencidos: con vencimiento anterior a hoy.
        // - Atrasados: sin vencimiento y con (FechaFin ?? FechaInicio) anterior a hoy.
        // - Hoy: vencen hoy, o sin vencimiento ocurren hoy (incluye eventos de varios días en curso).
        // - Próximos: vencen o empiezan después de hoy.
        var entradasActivas = _dbContext
            .EntradasAgenda.AsNoTracking()
            .Where(x => x.Activo)
            .Where(CriteriosAgenda.Abierta());

        var vencidos = await entradasActivas
            .CountAsync(CriteriosAgenda.Vencida(hoy), cancellationToken);
        var atrasados = await entradasActivas
            .CountAsync(CriteriosAgenda.Atrasada(hoy), cancellationToken);
        var deHoy = await entradasActivas.CountAsync(
            x => x.FechaVencimiento.HasValue
                ? x.FechaVencimiento.Value == hoy
                : x.FechaInicio <= hoy && (x.FechaFin ?? x.FechaInicio) >= hoy,
            cancellationToken
        );
        var proximos = await entradasActivas.CountAsync(
            x => x.FechaVencimiento.HasValue
                ? x.FechaVencimiento.Value > hoy
                : x.FechaInicio > hoy,
            cancellationToken
        );

        var elementosProximos = await entradasActivas
            .Where(x => (x.FechaVencimiento ?? x.FechaFin ?? x.FechaInicio) >= hoy)
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
                EsDeHoy = x.FechaVencimiento.HasValue
                    ? x.FechaVencimiento.Value == hoy
                    : x.FechaInicio <= hoy && (x.FechaFin ?? x.FechaInicio) >= hoy,
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
            Hoy = deHoy,
            Proximos = proximos,
            Vencidos = vencidos,
            Atrasados = atrasados,
            ElementosProximos = elementosProximos,
        };
    }
}
