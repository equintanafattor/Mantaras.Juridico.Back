using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.DiasInhabiles;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public class DiaInhabilRepository : IDiaInhabilRepository
{
    // Primer componente del advisory lock; el segundo es el año.
    private const int ClaveBloqueoImportacion = 27399;

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

    public async Task<IReadOnlyCollection<DiaInhabil>> ObtenerParaImportacionAsync(
        int anio,
        CancellationToken cancellationToken = default
    )
    {
        var desde = new DateOnly(anio, 1, 1);
        var hasta = new DateOnly(anio, 12, 31);

        return await _dbContext.DiasInhabiles
            .Where(x =>
                (x.Fecha >= desde && x.Fecha <= hasta)
                || (x.FechaFuente >= desde && x.FechaFuente <= hasta)
            )
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.DiaInhabilId)
            .ToListAsync(cancellationToken);
    }

    public Task<CalendarioFeriadosImportado?> ObtenerCalendarioAsync(
        int anio,
        bool seguimiento,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.CalendariosFeriadosImportados.AsQueryable();

        if (!seguimiento)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(x => x.Anio == anio, cancellationToken);
    }

    public async Task AgregarCalendarioAsync(
        CalendarioFeriadosImportado calendario,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.CalendariosFeriadosImportados.AddAsync(calendario, cancellationToken);
    }

    public async Task<(int Total, int Importados, int Manuales)> ContarActivosPorOrigenAsync(
        int anio,
        CancellationToken cancellationToken = default
    )
    {
        var conteos = await ConstruirConsulta(anio, soloActivos: true)
            .GroupBy(x => x.Origen)
            .Select(x => new { Origen = x.Key, Cantidad = x.Count() })
            .ToListAsync(cancellationToken);

        var importados = conteos
            .Where(x => x.Origen == OrigenDiaInhabil.FeriadoNacional)
            .Sum(x => x.Cantidad);
        var manuales = conteos
            .Where(x => x.Origen == OrigenDiaInhabil.Manual)
            .Sum(x => x.Cantidad);

        return (importados + manuales, importados, manuales);
    }

    public async Task<ITransaccionImportacionFeriados> IniciarImportacionExclusivaAsync(
        int anio,
        CancellationToken cancellationToken = default
    )
    {
        var transaccion = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // El bloqueo se libera solo al terminar la transacción.
            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock({ClaveBloqueoImportacion}, {anio})",
                cancellationToken
            );
        }
        catch
        {
            await transaccion.DisposeAsync();
            throw;
        }

        return new TransaccionImportacion(transaccion);
    }

    public Task GuardarImportacionAsync(CancellationToken cancellationToken = default) =>
        ViolacionUnicidad.GuardarAsync(_dbContext, cancellationToken);

    public void DescartarCambios() => _dbContext.ChangeTracker.Clear();

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

    private sealed class TransaccionImportacion : ITransaccionImportacionFeriados
    {
        private readonly IDbContextTransaction _transaccion;

        public TransaccionImportacion(IDbContextTransaction transaccion)
        {
            _transaccion = transaccion;
        }

        public Task ConfirmarAsync(CancellationToken cancellationToken = default) =>
            _transaccion.CommitAsync(cancellationToken);

        public ValueTask DisposeAsync() => _transaccion.DisposeAsync();
    }
}
