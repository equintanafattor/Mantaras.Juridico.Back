using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.Catalogos.Exceptions;
using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public class TipoExpedienteAdministrativoRepository : ITipoExpedienteAdministrativoRepository
{
    private readonly JuridicoDbContext _dbContext;

    public TipoExpedienteAdministrativoRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TipoExpedienteAdministrativo?> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => _dbContext.TiposExpedienteAdministrativo.FirstOrDefaultAsync(
        x => x.TipoExpedienteAdministrativoId == id, cancellationToken
    );

    public Task<bool> ExisteNombreAsync(
        string nombreNormalizado,
        long? idExcluir = null,
        CancellationToken cancellationToken = default
    ) => _dbContext.TiposExpedienteAdministrativo.AnyAsync(
        x => x.Nombre == nombreNormalizado
            && (!idExcluir.HasValue || x.TipoExpedienteAdministrativoId != idExcluir.Value),
        cancellationToken
    );

    public async Task AgregarAsync(
        TipoExpedienteAdministrativo entidad,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.TiposExpedienteAdministrativo.AddAsync(entidad, cancellationToken);
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
            && postgres.ConstraintName == "IX_TiposExpedienteAdministrativo_Nombre"
        )
        {
            // Evita dejar un alta/edición fallida pendiente en este contexto.
            foreach (var entry in exception.Entries)
            {
                if (entry.Entity is TipoExpedienteAdministrativo)
                {
                    entry.State = EntityState.Detached;
                }
            }

            throw new NombreCatalogoDuplicadoException(exception);
        }
    }

    public async Task<IReadOnlyCollection<TipoExpedienteAdministrativo>> BuscarAsync(
        string? busqueda,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await ConstruirConsulta(busqueda, soloActivos)
            .OrderBy(x => x.Nombre)
            .ThenBy(x => x.TipoExpedienteAdministrativoId)
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

    private IQueryable<TipoExpedienteAdministrativo> ConstruirConsulta(string? busqueda, bool soloActivos)
    {
        var query = _dbContext.TiposExpedienteAdministrativo.AsQueryable();

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
