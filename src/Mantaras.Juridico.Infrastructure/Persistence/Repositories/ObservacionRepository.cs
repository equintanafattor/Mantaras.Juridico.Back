using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public sealed class ObservacionRepository : IObservacionRepository
{
    private readonly JuridicoDbContext _dbContext;

    public ObservacionRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Observacion>>
        ObtenerPorClienteIdAsync(
            long clienteId,
            CancellationToken cancellationToken = default
        )
    {
        return await _dbContext
            .Observaciones
            .AsNoTracking()
            .Where(observacion => observacion.ClienteId == clienteId)
            .OrderByDescending(observacion => observacion.FechaCreacion)
            .ThenByDescending(observacion => observacion.ObservacionId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Observacion>>
        ObtenerPorCasoIdAsync(
            long casoId,
            CancellationToken cancellationToken = default
        )
    {
        return await _dbContext
            .Observaciones
            .AsNoTracking()
            .Where(observacion => observacion.CasoId == casoId)
            .OrderByDescending(observacion => observacion.FechaCreacion)
            .ThenByDescending(observacion => observacion.ObservacionId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Observacion>>
        ObtenerPorExpedienteIdAsync(
            long expedienteId,
            CancellationToken cancellationToken = default
        )
    {
        return await _dbContext
            .Observaciones
            .AsNoTracking()
            .Where(observacion => observacion.ExpedienteId == expedienteId)
            .OrderByDescending(observacion => observacion.FechaCreacion)
            .ThenByDescending(observacion => observacion.ObservacionId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AgregarAsync(
        Observacion observacion,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.Observaciones.AddAsync(
            observacion,
            cancellationToken
        );
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}