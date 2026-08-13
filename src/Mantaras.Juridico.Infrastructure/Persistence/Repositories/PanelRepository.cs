using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Domain.Entities;
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
            .Include(x => x.Caso)
            .Where(x => x.Activo && x.Caso.Activo)
            .OrderByDescending(
                x => x.FechaModificacion ?? x.FechaCreacion
            )
            .Take(cantidad)
            .ToListAsync(cancellationToken);
    }
}