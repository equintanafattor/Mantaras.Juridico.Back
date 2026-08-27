using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.Catalogos.Exceptions;
using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public class TipoBeneficioRepository : ITipoBeneficioRepository
{
    private readonly JuridicoDbContext _dbContext;

    public TipoBeneficioRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TipoBeneficio?> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => _dbContext.TiposBeneficio.FirstOrDefaultAsync(
        x => x.TipoBeneficioId == id, cancellationToken
    );

    public Task<bool> ExisteNombreAsync(
        string nombreNormalizado,
        long? idExcluir = null,
        CancellationToken cancellationToken = default
    ) => _dbContext.TiposBeneficio.AnyAsync(
        x => x.Nombre == nombreNormalizado
            && (!idExcluir.HasValue || x.TipoBeneficioId != idExcluir.Value),
        cancellationToken
    );

    public async Task AgregarAsync(
        TipoBeneficio entidad,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.TiposBeneficio.AddAsync(entidad, cancellationToken);
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
            && postgres.ConstraintName == "IX_TiposBeneficio_Nombre"
        )
        {
            // Evita dejar un alta/edición fallida pendiente en este contexto.
            foreach (var entry in exception.Entries)
            {
                if (entry.Entity is TipoBeneficio)
                {
                    entry.State = EntityState.Detached;
                }
            }

            throw new NombreCatalogoDuplicadoException(exception);
        }
    }

    public async Task<IReadOnlyCollection<TipoBeneficio>> BuscarAsync(
        string? busqueda,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await ConstruirConsulta(busqueda, soloActivos)
            .OrderBy(x => x.Nombre)
            .ThenBy(x => x.TipoBeneficioId)
            .Skip(checked((page - 1) * pageSize))
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<int> ContarAsync(
        string? busqueda,
        bool soloActivos,
        CancellationToken cancellationToken = default
    ) => ConstruirConsulta(busqueda, soloActivos).CountAsync(cancellationToken);

    private IQueryable<TipoBeneficio> ConstruirConsulta(string? busqueda, bool soloActivos)
    {
        var query = _dbContext.TiposBeneficio.AsQueryable();

        if (soloActivos)
        {
            query = query.Where(x => x.Activo);
        }

        var termino = NombreCatalogo.Normalizar(busqueda);

        if (termino.Length > 0)
        {
            // %, _ y barra invertida se buscan literalmente, no como comodines.
            var escapado = termino.Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
            var patron = $"%{escapado}%";

            query = query.Where(x => EF.Functions.ILike(x.Nombre, patron, "\\"));
        }

        return query;
    }
}
