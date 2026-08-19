using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public sealed class ExpedienteRepository : IExpedienteRepository
{
    private readonly JuridicoDbContext _dbContext;

    public ExpedienteRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Expediente?> ObtenerPorIdAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext
            .Expedientes.Include(x => x.Caso)
            .FirstOrDefaultAsync(
                x => x.ExpedienteId == expedienteId,
                cancellationToken
            );
    }

    public Task<Expediente?> ObtenerDetallePorIdAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext
            .Expedientes.AsNoTracking()
            .Include(x => x.Caso)
            .Include(x => x.ExpedientePadre)
            .Include(x => x.ExpedientesDerivados)
            .FirstOrDefaultAsync(
                x => x.ExpedienteId == expedienteId,
                cancellationToken
            );
    }

    public async Task AgregarAsync(
        Expediente expediente,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.Expedientes.AddAsync(
            expediente,
            cancellationToken
        );
    }

    public async Task<IReadOnlyCollection<Expediente>> BuscarAsync(
        long? casoId,
        string? busqueda,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = ConstruirConsulta(
            casoId,
            busqueda,
            soloActivos
        );

        return await query
            .Include(x => x.Caso)
            .OrderByDescending(x => x.FechaCreacion)
            .ThenBy(x => x.Caratula)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<int> ContarAsync(
        long? casoId,
        string? busqueda,
        bool soloActivos,
        CancellationToken cancellationToken = default
    )
    {
        return ConstruirConsulta(
            casoId,
            busqueda,
            soloActivos
        ).CountAsync(cancellationToken);
    }

    public Task<bool> TieneDerivadosActivosAsync(
        long expedienteId,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.Expedientes.AnyAsync(
            x =>
                x.ExpedientePadreId == expedienteId
                && x.Activo,
            cancellationToken
        );
    }

    public Task<bool> ExistePrincipalAsync(
        long casoId,
        long? expedienteIdExcluir = null,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.Expedientes.AnyAsync(
            x =>
                x.CasoId == casoId
                && x.TipoExpediente == TipoExpediente.Principal
                && (
                    !expedienteIdExcluir.HasValue
                    || x.ExpedienteId != expedienteIdExcluir.Value
                ),
            cancellationToken
        );
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Expediente> ConstruirConsulta(
        long? casoId,
        string? busqueda,
        bool soloActivos
    )
    {
        var query = _dbContext.Expedientes.AsQueryable();

        if (soloActivos)
        {
            query = query.Where(x => x.Activo);
        }

        if (casoId.HasValue)
        {
            query = query.Where(x => x.CasoId == casoId.Value);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();

            query = query.Where(x =>
                (
                    x.NumeroExpediente != null
                    && EF.Functions.ILike(
                        x.NumeroExpediente,
                        $"%{termino}%"
                    )
                )
                || EF.Functions.ILike(
                    x.Caratula,
                    $"%{termino}%"
                )
                || (
                    x.Juzgado != null
                    && EF.Functions.ILike(
                        x.Juzgado,
                        $"%{termino}%"
                    )
                )
                || (
                    x.EstadoLegal != null
                    && EF.Functions.ILike(
                        x.EstadoLegal,
                        $"%{termino}%"
                    )
                )
                || (
                    x.Observaciones != null
                    && EF.Functions.ILike(
                        x.Observaciones,
                        $"%{termino}%"
                    )
                )
                || EF.Functions.ILike(
                    x.Caso.Titulo,
                    $"%{termino}%"
                )
                || x.Caso.Clientes.Any(relacion =>
                    EF.Functions.ILike(
                        relacion.Cliente.Nombre,
                        $"%{termino}%"
                    )
                    || EF.Functions.ILike(
                        relacion.Cliente.Apellido,
                        $"%{termino}%"
                    )
                    || (
                        relacion.Cliente.Dni != null
                        && EF.Functions.ILike(
                            relacion.Cliente.Dni,
                            $"%{termino}%"
                        )
                    )
                    || (
                        relacion.Cliente.Cuil != null
                        && EF.Functions.ILike(
                            relacion.Cliente.Cuil,
                            $"%{termino}%"
                        )
                    )
                )
            );
        }

        return query;
    }
}