using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.DiasInhabiles;
using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public class DiaInhabilRepository : IDiaInhabilRepository
{
    private readonly JuridicoDbContext _dbContext;

    public DiaInhabilRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<DiaInhabil?> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => _dbContext.DiasInhabiles.FirstOrDefaultAsync(
        x => x.DiaInhabilId == id, cancellationToken
    );

    public Task<bool> ExisteFechaAsync(
        DateOnly fecha,
        long? idExcluir = null,
        CancellationToken cancellationToken = default
    ) => _dbContext.DiasInhabiles.AnyAsync(
        x => x.Fecha == fecha
            && (!idExcluir.HasValue || x.DiaInhabilId != idExcluir.Value),
        cancellationToken
    );

    public async Task AgregarAsync(
        DiaInhabil entidad,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.DiasInhabiles.AddAsync(entidad, cancellationToken);
    }

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException postgres
            && postgres.SqlState == PostgresErrorCodes.UniqueViolation
            && postgres.ConstraintName == "IX_DiasInhabiles_Fecha"
        )
        {
            // Evita dejar un alta/edición fallida pendiente en este contexto.
            foreach (var entry in exception.Entries)
            {
                if (entry.Entity is DiaInhabil)
                {
                    entry.State = EntityState.Detached;
                }
            }

            throw new FechaDiaInhabilDuplicadaException(exception);
        }
    }

    public async Task<IReadOnlyCollection<DiaInhabil>> BuscarAsync(
        int? anio,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await ConstruirConsulta(anio, soloActivos)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.DiaInhabilId)
            .Skip(checked((page - 1) * pageSize))
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<int> ContarAsync(
        int? anio,
        bool soloActivos,
        CancellationToken cancellationToken = default
    ) => ConstruirConsulta(anio, soloActivos).CountAsync(cancellationToken);

    private IQueryable<DiaInhabil> ConstruirConsulta(int? anio, bool soloActivos)
    {
        var query = _dbContext.DiasInhabiles.AsQueryable();

        if (soloActivos)
        {
            query = query.Where(x => x.Activo);
        }

        if (anio.HasValue)
        {
            var desde = new DateOnly(anio.Value, 1, 1);
            var hasta = new DateOnly(anio.Value, 12, 31);
            query = query.Where(x => x.Fecha >= desde && x.Fecha <= hasta);
        }

        return query;
    }
}
