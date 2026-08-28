using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public sealed class RelacionFamiliarRepository : IRelacionFamiliarRepository
{
    private readonly JuridicoDbContext _dbContext;

    public RelacionFamiliarRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<RelacionFamiliar>> ListarPorClienteAsync(
        long clienteId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.RelacionesFamiliares
            .AsNoTracking()
            .Where(x =>
                x.Activo
                && (x.ClienteAId == clienteId || x.ClienteBId == clienteId)
            )
            .Include(x => x.ClienteA)
            .Include(x => x.ClienteB)
            .ToListAsync(cancellationToken);
    }

    public Task<RelacionFamiliar?> ObtenerPorParejaAsync(
        long clienteAId,
        long clienteBId,
        CancellationToken cancellationToken = default
    )
    {
        // Incluye relaciones inactivas para poder reactivarlas.
        return _dbContext.RelacionesFamiliares
            .FirstOrDefaultAsync(
                x => x.ClienteAId == clienteAId
                    && x.ClienteBId == clienteBId,
                cancellationToken
            );
    }

    public async Task AgregarAsync(
        RelacionFamiliar relacion,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.RelacionesFamiliares.AddAsync(
            relacion,
            cancellationToken
        );
    }

    public async Task<bool> IntentarGuardarCambiosAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception)
            when (
                exception.InnerException is PostgresException postgres
                && postgres.SqlState == PostgresErrorCodes.UniqueViolation
                && postgres.ConstraintName
                    == "IX_RelacionesFamiliares_ClienteAId_ClienteBId"
            )
        {
            return false;
        }
    }
}