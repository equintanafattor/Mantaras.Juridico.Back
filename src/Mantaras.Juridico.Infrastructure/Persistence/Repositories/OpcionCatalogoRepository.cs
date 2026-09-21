using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Application.Features.Catalogos.Exceptions;
using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public sealed class OpcionCatalogoRepository : IOpcionCatalogoRepository
{
    private readonly JuridicoDbContext _dbContext;

    public OpcionCatalogoRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<OpcionCatalogo?> ObtenerPorIdAsync(
        string tipo,
        long id,
        CancellationToken cancellationToken = default
    ) => _dbContext.OpcionesCatalogo.FirstOrDefaultAsync(
        x => x.Tipo == tipo && x.OpcionCatalogoId == id,
        cancellationToken
    );

    public Task<bool> ExisteNombreAsync(
        string tipo,
        string nombre,
        long? idExcluir = null,
        CancellationToken cancellationToken = default
    ) => _dbContext.OpcionesCatalogo.AnyAsync(
        x => x.Tipo == tipo
            && x.Nombre == nombre
            && (!idExcluir.HasValue || x.OpcionCatalogoId != idExcluir.Value),
        cancellationToken
    );

    public Task<bool> ExisteActivoAsync(
        string tipo,
        string nombre,
        CancellationToken cancellationToken = default
    ) => _dbContext.OpcionesCatalogo.AnyAsync(
        x => x.Tipo == tipo && x.Nombre == nombre && x.Activo,
        cancellationToken
    );

    public async Task AgregarAsync(
        OpcionCatalogo entidad,
        CancellationToken cancellationToken = default
    ) => await _dbContext.OpcionesCatalogo.AddAsync(entidad, cancellationToken);

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException postgres
            && postgres.SqlState == PostgresErrorCodes.UniqueViolation
        )
        {
            throw new NombreCatalogoDuplicadoException(exception);
        }
    }

    public async Task<IReadOnlyCollection<OpcionCatalogo>> BuscarAsync(
        string tipo,
        string? busqueda,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    ) => await ConstruirConsulta(tipo, busqueda, soloActivos)
        .OrderBy(x => x.Nombre)
        .ThenBy(x => x.OpcionCatalogoId)
        .Skip(checked((page - 1) * pageSize))
        .Take(pageSize)
        .AsNoTracking()
        .ToListAsync(cancellationToken);

    public Task<int> ContarAsync(
        string tipo,
        string? busqueda,
        bool soloActivos,
        CancellationToken cancellationToken = default
    ) => ConstruirConsulta(tipo, busqueda, soloActivos)
        .CountAsync(cancellationToken);

    private IQueryable<OpcionCatalogo> ConstruirConsulta(
        string tipo,
        string? busqueda,
        bool soloActivos
    )
    {
        var query = _dbContext.OpcionesCatalogo.Where(x => x.Tipo == tipo);

        if (soloActivos)
        {
            query = query.Where(x => x.Activo);
        }

        var termino = NombreCatalogo.Normalizar(busqueda);
        if (termino.Length > 0)
        {
            var escapado = termino.Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
            query = query.Where(x =>
                EF.Functions.ILike(x.Nombre, $"%{escapado}%", "\\")
            );
        }

        return query;
    }
}
