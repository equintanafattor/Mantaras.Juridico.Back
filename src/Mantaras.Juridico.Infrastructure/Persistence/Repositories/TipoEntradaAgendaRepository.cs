using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.Catalogos.Exceptions;
using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public class TipoEntradaAgendaRepository : ITipoEntradaAgendaRepository
{
    private readonly JuridicoDbContext _dbContext;

    public TipoEntradaAgendaRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TipoEntradaAgenda?> ObtenerPorIdAsync(
        long id,
        CancellationToken cancellationToken = default
    ) => _dbContext.TiposEntradaAgenda.FirstOrDefaultAsync(
        x => x.TipoEntradaAgendaId == id, cancellationToken
    );

    public Task<bool> ExisteNombreAsync(
        string nombreNormalizado,
        long? idExcluir = null,
        CancellationToken cancellationToken = default
    ) => _dbContext.TiposEntradaAgenda.AnyAsync(
        x => x.Nombre.ToUpper() == nombreNormalizado
            && (!idExcluir.HasValue || x.TipoEntradaAgendaId != idExcluir.Value),
        cancellationToken
    );

    public async Task AgregarAsync(
        TipoEntradaAgenda entidad,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.TiposEntradaAgenda.AddAsync(entidad, cancellationToken);
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
            && postgres.ConstraintName == "IX_TiposEntradaAgenda_Nombre"
        )
        {
            // Evita dejar un alta/edición fallida pendiente en este contexto.
            foreach (var entry in exception.Entries)
            {
                if (entry.Entity is TipoEntradaAgenda)
                {
                    entry.State = EntityState.Detached;
                }
            }

            throw new NombreCatalogoDuplicadoException(exception);
        }
    }

    public async Task<IReadOnlyCollection<TipoEntradaAgenda>> BuscarAsync(
        string? busqueda,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await ConstruirConsulta(busqueda, soloActivos)
            .OrderBy(x => x.Nombre)
            .ThenBy(x => x.TipoEntradaAgendaId)
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

    private IQueryable<TipoEntradaAgenda> ConstruirConsulta(string? busqueda, bool soloActivos)
    {
        var query = _dbContext.TiposEntradaAgenda.AsQueryable();

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
