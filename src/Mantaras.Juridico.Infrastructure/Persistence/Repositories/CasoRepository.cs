using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
            .FirstOrDefaultAsync(x => x.CasoId == casoId, cancellationToken);
    }

    public async Task AgregarAsync(Caso caso, CancellationToken cancellationToken = default)
    {
        await _dbContext.Casos.AddAsync(caso, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Caso>> BuscarAsync(
        string? busqueda,
        FaseCaso? faseInterna,
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
        FaseCaso? faseInterna,
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
        return _dbContext.Expedientes.AnyAsync(
            x => x.CasoId == casoId && x.Activo,
            cancellationToken
        );
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Caso> ConstruirConsulta(
        string? busqueda,
        FaseCaso? faseInterna,
        bool soloActivos
    )
    {
        var query = _dbContext.Casos.AsQueryable();

        if (soloActivos)
        {
            query = query.Where(x => x.Activo);
        }

        if (faseInterna.HasValue)
        {
            query = query.Where(x => x.FaseInterna == faseInterna.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Titulo, $"%{termino}%")
                || (x.TipoTramite != null && EF.Functions.ILike(x.TipoTramite, $"%{termino}%"))
                || (x.NumeroExpedienteAnses != null
                    && EF.Functions.ILike(x.NumeroExpedienteAnses, $"%{termino}%"))
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
}
