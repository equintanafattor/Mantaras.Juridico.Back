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
            .Expedientes
            .Include(x => x.Casos)
                .ThenInclude(x => x.Caso)
                    .ThenInclude(x => x.TipoBeneficio)
            .Include(x => x.EnviosCobro.Where(envio => envio.Activo))
                .ThenInclude(x => x.AgendaGeneracionRegla)
                    .ThenInclude(x => x.EntradaAgenda)
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
            .Include(x => x.Casos)
                .ThenInclude(x => x.Caso)
                    .ThenInclude(x => x.TipoBeneficio)
            .Include(x => x.EnviosCobro.Where(envio => envio.Activo))
                .ThenInclude(x => x.AgendaGeneracionRegla)
                    .ThenInclude(x => x.EntradaAgenda)
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
            .Include(x => x.Casos)
                .ThenInclude(x => x.Caso)
                    .ThenInclude(x => x.TipoBeneficio)
            .Include(x => x.EnviosCobro.Where(envio => envio.Activo))
                .ThenInclude(x => x.AgendaGeneracionRegla)
                    .ThenInclude(x => x.EntradaAgenda)
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

    public async Task<IReadOnlyCollection<Expediente>> BuscarCobroAsync(
        string? busqueda,
        bool soloActivos,
        bool? seguimientoVencido,
        DateOnly fechaActual,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await ConstruirConsultaCobro(
            busqueda,
            soloActivos,
            seguimientoVencido,
            fechaActual
        )
            .Include(x => x.Casos)
                .ThenInclude(x => x.Caso)
                    .ThenInclude(x => x.TipoBeneficio)
            .Include(x => x.EnviosCobro.Where(envio => envio.Activo))
                .ThenInclude(x => x.AgendaGeneracionRegla)
                    .ThenInclude(x => x.EntradaAgenda)
            .OrderBy(x => x.EnviosCobro
                .Where(envio => envio.Activo)
                .Min(envio =>
                    envio.AgendaGeneracionRegla.EntradaAgenda.FechaVencimiento
                ))
            .ThenBy(x => x.ExpedienteId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<int> ContarCobroAsync(
        string? busqueda,
        bool soloActivos,
        bool? seguimientoVencido,
        DateOnly fechaActual,
        CancellationToken cancellationToken = default
    )
    {
        return ConstruirConsultaCobro(
            busqueda,
            soloActivos,
            seguimientoVencido,
            fechaActual
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
        IReadOnlyCollection<long> casoIds,
        long? expedienteIdExcluir = null,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.Expedientes.AnyAsync(
            x =>
                x.TipoExpediente == TipoExpediente.Principal
                && x.Casos.Any(relacion => casoIds.Contains(relacion.CasoId))
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
        return ViolacionUnicidad.GuardarAsync(_dbContext, cancellationToken);
    }

    public void DescartarCambios()
    {
        _dbContext.ChangeTracker.Clear();
    }

    private IQueryable<Expediente> ConstruirConsultaCobro(
        string? busqueda,
        bool soloActivos,
        bool? seguimientoVencido,
        DateOnly fechaActual
    )
    {
        var query = ConstruirConsulta(null, busqueda, soloActivos);

        return seguimientoVencido switch
        {
            // Mismo criterio que Agenda: vencido = entrada abierta con fecha pasada.
            true => query.Where(x => x.EnviosCobro.Any(envio =>
                envio.Activo
                && envio.AgendaGeneracionRegla.EntradaAgenda.Estado != EstadoEntradaAgenda.Completada
                && envio.AgendaGeneracionRegla.EntradaAgenda.Estado != EstadoEntradaAgenda.Cancelada
                && envio.AgendaGeneracionRegla.EntradaAgenda.FechaVencimiento
                    < fechaActual
            )),
            // No vencidos: abiertos sin vencer y también los cerrados.
            false => query.Where(x => x.EnviosCobro.Any(envio =>
                envio.Activo
                && (
                    envio.AgendaGeneracionRegla.EntradaAgenda.Estado == EstadoEntradaAgenda.Completada
                    || envio.AgendaGeneracionRegla.EntradaAgenda.Estado == EstadoEntradaAgenda.Cancelada
                    || envio.AgendaGeneracionRegla.EntradaAgenda.FechaVencimiento
                        >= fechaActual
                )
            )),
            null => query.Where(x => x.EnviosCobro.Any(envio => envio.Activo)),
        };
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
            query = query.Where(x =>
                x.Casos.Any(relacion => relacion.CasoId == casoId.Value)
            );
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
                ) || x.HistorialObservaciones.Any(
                    observacion =>
                        EF.Functions.ILike(
                            observacion.Texto,
                            $"%{termino}%"
                        )
                )
                || x.Casos.Any(casoExpediente =>
                    EF.Functions.ILike(
                        casoExpediente.Caso.Titulo,
                        $"%{termino}%"
                    )
                    || casoExpediente.Caso.Clientes.Any(relacion =>
                        EF.Functions.ILike(
                            relacion.Cliente.Nombre,
                            $"%{termino}%"
                        )
                        || EF.Functions.ILike(
                            relacion.Cliente.Apellido,
                            $"%{termino}%"
                        )
                        || EF.Functions.ILike(
                            relacion.Cliente.Apellido + ", " + relacion.Cliente.Nombre,
                            $"%{termino}%"
                        )
                        || EF.Functions.ILike(
                            relacion.Cliente.Nombre + " " + relacion.Cliente.Apellido,
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
                )
            );
        }

        return query;
    }
}
