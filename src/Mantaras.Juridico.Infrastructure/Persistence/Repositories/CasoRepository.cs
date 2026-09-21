using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public sealed class CasoRepository : ICasoRepository
{
    private readonly JuridicoDbContext _dbContext;

    public CasoRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Caso?> ObtenerPorIdAsync(long casoId, CancellationToken cancellationToken = default)
    {
        return _dbContext
            .Casos
            .Include(x => x.TipoBeneficio)
            .Include(x => x.TipoExpedienteAdministrativo)
            .Include(x => x.Clientes)
                .ThenInclude(x => x.Cliente)
            .FirstOrDefaultAsync(x => x.CasoId == casoId, cancellationToken);
    }

    public Task<Caso?> ObtenerDetallePorIdAsync(
    long casoId,
    CancellationToken cancellationToken = default
)
    {
        return _dbContext
            .Casos.AsNoTracking()
            .Include(x => x.TipoBeneficio)
            .Include(x => x.TipoExpedienteAdministrativo)
            .Include(x => x.Clientes)
                .ThenInclude(x => x.Cliente)
            .Include(x => x.Expedientes)
                .ThenInclude(x => x.Expediente)
            .FirstOrDefaultAsync(x => x.CasoId == casoId, cancellationToken);
    }

    public async Task AgregarAsync(Caso caso, CancellationToken cancellationToken = default)
    {
        await _dbContext.Casos.AddAsync(caso, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Caso>> ObtenerActivosPorIdsAsync(
        IReadOnlyCollection<long> casoIds,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbContext.Casos
            .Where(x => casoIds.Contains(x.CasoId) && x.Activo)
            .Include(x => x.TipoBeneficio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Caso>> BuscarAsync(
        string? busqueda,
        string? faseInterna,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = ConstruirConsulta(busqueda, faseInterna, soloActivos);

        return await query
            .Include(x => x.TipoBeneficio)
            .Include(x => x.TipoExpedienteAdministrativo)
            .Include(x => x.Clientes)
                .ThenInclude(x => x.Cliente)
            .OrderByDescending(x => x.FechaCreacion)
            .ThenBy(x => x.Titulo)
            .ThenBy(x => x.CasoId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<int> ContarAsync(
        string? busqueda,
        string? faseInterna,
        bool soloActivos,
        CancellationToken cancellationToken = default
    )
    {
        return ConstruirConsulta(busqueda, faseInterna, soloActivos).CountAsync(cancellationToken);
    }

    public Task<bool> TieneExpedientesActivosAsync(
        long casoId,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.CasosExpedientes.AnyAsync(
            x => x.CasoId == casoId && x.Expediente.Activo,
            cancellationToken
        );
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Caso> ConstruirConsulta(
        string? busqueda,
        string? faseInterna,
        bool soloActivos
    )
    {
        var query = _dbContext.Casos.AsQueryable();

        if (soloActivos)
        {
            query = query.Where(x => x.Activo);
        }

        if (!string.IsNullOrWhiteSpace(faseInterna))
        {
            query = query.Where(x => x.FaseInterna == faseInterna.Trim());
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Titulo, $"%{termino}%")
                || (x.TipoTramite != null && EF.Functions.ILike(x.TipoTramite, $"%{termino}%"))
                || (x.NumeroExpedienteAnses != null
                    && EF.Functions.ILike(x.NumeroExpedienteAnses, $"%{termino}%"))
                || (x.NumeroBeneficio != null
                    && EF.Functions.ILike(x.NumeroBeneficio, $"%{termino}%"))
                || x.Clientes.Any(relacion =>
                    EF.Functions.ILike(relacion.Cliente.Nombre, $"%{termino}%")
                    || EF.Functions.ILike(relacion.Cliente.Apellido, $"%{termino}%")
                    || (
                        relacion.Cliente.Dni != null
                        && EF.Functions.ILike(relacion.Cliente.Dni, $"%{termino}%")
                    )
                    || (
                        relacion.Cliente.Cuil != null
                        && EF.Functions.ILike(relacion.Cliente.Cuil, $"%{termino}%")
                    )
                )
            );
        }

        return query;
    }

    public Task<Caso?> ObtenerConHojaResumenAsync(
    long casoId,
    CancellationToken cancellationToken = default
)
    {
        return _dbContext.Casos
            .Include(x => x.HojaResumen)
            .FirstOrDefaultAsync(
                x => x.CasoId == casoId,
                cancellationToken
            );
    }

    public async Task AgregarHojaResumenAsync(
        HojaResumenCaso hoja,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.HojasResumenCasos.AddAsync(
            hoja,
            cancellationToken
        );
    }

    public async Task<bool> GuardarHojaResumenAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException postgres
            && postgres.SqlState == PostgresErrorCodes.UniqueViolation
            && postgres.ConstraintName == "PK_HojasResumenCasos"
        )
        {
            return false;
        }
    }
}
