using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public sealed class AgendaRepository : IAgendaRepository
{
    private readonly JuridicoDbContext _dbContext;

    public AgendaRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<EntradaAgenda?> ObtenerPorIdAsync(
        long entradaAgendaId,
        bool seguimiento = true,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext
            .EntradasAgenda
            .Include(x => x.TipoEntrada)
            .Include(x => x.Clientes)
            .Include(x => x.Casos)
            .Include(x => x.Expedientes)
            .Include(x => x.Responsables)
            .AsQueryable();

        if (!seguimiento)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(
            x => x.EntradaAgendaId == entradaAgendaId,
            cancellationToken
        );
    }

    public Task<TipoEntradaAgenda?> ObtenerTipoPorIdAsync(
        long tipoEntradaAgendaId,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.TiposEntradaAgenda
            .Include(x => x.RecordatoriosPredeterminados)
            .FirstOrDefaultAsync(
                x => x.TipoEntradaAgendaId == tipoEntradaAgendaId,
                cancellationToken
            );
    }

    public Task<bool> ExistenClientesActivosAsync(
        IReadOnlyCollection<long> clienteIds,
        CancellationToken cancellationToken = default
    )
    {
        return CoincidenTodosAsync(
            clienteIds,
            _dbContext.Clientes.Where(x => x.Activo).Select(x => x.ClienteId),
            cancellationToken
        );
    }

    public Task<bool> ExistenCasosActivosAsync(
        IReadOnlyCollection<long> casoIds,
        CancellationToken cancellationToken = default
    )
    {
        return CoincidenTodosAsync(
            casoIds,
            _dbContext.Casos.Where(x => x.Activo).Select(x => x.CasoId),
            cancellationToken
        );
    }

    public Task<bool> ExistenExpedientesActivosAsync(
        IReadOnlyCollection<long> expedienteIds,
        CancellationToken cancellationToken = default
    )
    {
        return CoincidenTodosAsync(
            expedienteIds,
            _dbContext.Expedientes.Where(x => x.Activo).Select(x => x.ExpedienteId),
            cancellationToken
        );
    }

    public Task<bool> ExistenUsuariosActivosAsync(
        IReadOnlyCollection<long> usuarioIds,
        CancellationToken cancellationToken = default
    )
    {
        return CoincidenTodosAsync(
            usuarioIds,
            _dbContext.Users.Where(x => x.Activo).Select(x => x.Id),
            cancellationToken
        );
    }

    public async Task<IReadOnlyCollection<EntradaAgenda>> BuscarAsync(
        DateOnly? desde,
        DateOnly? hasta,
        long? tipoEntradaAgendaId,
        EstadoEntradaAgenda? estado,
        long? responsableId,
        long? clienteId,
        long? casoId,
        long? expedienteId,
        string? busqueda,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await ConstruirConsulta(
                desde,
                hasta,
                tipoEntradaAgendaId,
                estado,
                responsableId,
                clienteId,
                casoId,
                expedienteId,
                busqueda,
                soloActivos
            )
            .Include(x => x.TipoEntrada)
            .Include(x => x.Clientes)
                .ThenInclude(x => x.Cliente)
            .Include(x => x.Casos)
                .ThenInclude(x => x.Caso)
            .Include(x => x.Expedientes)
                .ThenInclude(x => x.Expediente)
            .Include(x => x.Responsables)
            .OrderBy(x => x.FechaInicio)
            .ThenBy(x => x.HoraInicio)
            .ThenBy(x => x.EntradaAgendaId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<int> ContarAsync(
        DateOnly? desde,
        DateOnly? hasta,
        long? tipoEntradaAgendaId,
        EstadoEntradaAgenda? estado,
        long? responsableId,
        long? clienteId,
        long? casoId,
        long? expedienteId,
        string? busqueda,
        bool soloActivos,
        CancellationToken cancellationToken = default
    )
    {
        return ConstruirConsulta(
            desde,
            hasta,
            tipoEntradaAgendaId,
            estado,
            responsableId,
            clienteId,
            casoId,
            expedienteId,
            busqueda,
            soloActivos
        ).CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<long, string>> ObtenerNombresUsuariosAsync(
        IReadOnlyCollection<long> usuarioIds,
        CancellationToken cancellationToken = default
    )
    {
        if (usuarioIds.Count == 0)
        {
            return new Dictionary<long, string>();
        }

        return await _dbContext.Users
            .Where(x => usuarioIds.Contains(x.Id))
            .AsNoTracking()
            .ToDictionaryAsync(
                x => x.Id,
                x => x.Nombre,
                cancellationToken
            );
    }

    public async Task<IReadOnlyCollection<ReglaVencimiento>> ObtenerReglasAsync(
        bool soloActivas,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.ReglasVencimiento.AsQueryable();

        if (soloActivas)
        {
            query = query.Where(x => x.Activo);
        }

        return await query
            .Include(x => x.TipoEntrada)
            .OrderByDescending(x => x.Activo)
            .ThenBy(x => x.Nombre)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<ReglaVencimiento?> ObtenerReglaPorIdAsync(
        long reglaVencimientoId,
        bool seguimiento = true,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.ReglasVencimiento
            .Include(x => x.TipoEntrada)
                .ThenInclude(x => x.RecordatoriosPredeterminados)
            .Include(x => x.RecordatoriosPredeterminados)
            .AsQueryable();

        if (!seguimiento)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(
            x => x.ReglaVencimientoId == reglaVencimientoId,
            cancellationToken
        );
    }

    public Task<bool> ExisteNombreReglaAsync(
        string nombre,
        long? reglaVencimientoIdExcluir = null,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.ReglasVencimiento.AnyAsync(
            x =>
                EF.Functions.ILike(x.Nombre, nombre)
                && (
                    !reglaVencimientoIdExcluir.HasValue
                    || x.ReglaVencimientoId != reglaVencimientoIdExcluir.Value
                ),
            cancellationToken
        );
    }

    public Task<AgendaGeneracionRegla?> ObtenerGeneracionPorClaveAsync(
        string claveIdempotencia,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.AgendaGeneracionesReglas
            .Include(x => x.ReglaVencimiento)
            .Include(x => x.EntradaAgenda)
                .ThenInclude(x => x.TipoEntrada)
            .Include(x => x.EntradaAgenda)
                .ThenInclude(x => x.Clientes)
            .Include(x => x.EntradaAgenda)
                .ThenInclude(x => x.Casos)
            .Include(x => x.EntradaAgenda)
                .ThenInclude(x => x.Expedientes)
            .Include(x => x.EntradaAgenda)
                .ThenInclude(x => x.Responsables)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ClaveIdempotencia == claveIdempotencia,
                cancellationToken
            );
    }

    public Task<Observacion?> ObtenerObservacionPorIdAsync(
        long observacionId,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.Observaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ObservacionId == observacionId,
                cancellationToken
            );
    }

    public async Task<IReadOnlyCollection<DateOnly>>
        ObtenerDiasInhabilesActivosAsync(
            CancellationToken cancellationToken = default
        )
    {
        return await _dbContext.DiasInhabiles
            .AsNoTracking()
            .Where(x => x.Activo)
            .Select(x => x.Fecha)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AgregarReglaAsync(
        ReglaVencimiento regla,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.ReglasVencimiento.AddAsync(
            regla,
            cancellationToken
        );
    }

    public async Task AgregarGeneracionAsync(
        AgendaGeneracionRegla generacion,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.AgendaGeneracionesReglas.AddAsync(
            generacion,
            cancellationToken
        );
    }

    public async Task AgregarAsync(
        EntradaAgenda entrada,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.EntradasAgenda.AddAsync(entrada, cancellationToken);
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<bool> CoincidenTodosAsync(
        IReadOnlyCollection<long> ids,
        IQueryable<long> query,
        CancellationToken cancellationToken
    )
    {
        if (ids.Count == 0)
        {
            return true;
        }

        var unicos = ids.Distinct().ToArray();
        var cantidad = await query.CountAsync(
            id => unicos.Contains(id),
            cancellationToken
        );

        return cantidad == unicos.Length;
    }

    private IQueryable<EntradaAgenda> ConstruirConsulta(
        DateOnly? desde,
        DateOnly? hasta,
        long? tipoEntradaAgendaId,
        EstadoEntradaAgenda? estado,
        long? responsableId,
        long? clienteId,
        long? casoId,
        long? expedienteId,
        string? busqueda,
        bool soloActivos
    )
    {
        var query = _dbContext.EntradasAgenda.AsQueryable();

        if (soloActivos)
        {
            query = query.Where(x => x.Activo);
        }

        if (desde.HasValue)
        {
            query = query.Where(x => (x.FechaFin ?? x.FechaInicio) >= desde.Value);
        }

        if (hasta.HasValue)
        {
            query = query.Where(x => x.FechaInicio <= hasta.Value);
        }

        if (tipoEntradaAgendaId.HasValue)
        {
            query = query.Where(x =>
                x.TipoEntradaAgendaId == tipoEntradaAgendaId.Value
            );
        }

        if (estado.HasValue)
        {
            query = query.Where(x => x.Estado == estado.Value);
        }

        if (responsableId.HasValue)
        {
            query = query.Where(x =>
                x.Responsables.Any(r => r.UsuarioId == responsableId.Value)
            );
        }

        if (clienteId.HasValue)
        {
            query = query.Where(x =>
                x.Clientes.Any(r => r.ClienteId == clienteId.Value)
            );
        }

        if (casoId.HasValue)
        {
            query = query.Where(x =>
                x.Casos.Any(r => r.CasoId == casoId.Value)
            );
        }

        if (expedienteId.HasValue)
        {
            query = query.Where(x =>
                x.Expedientes.Any(r => r.ExpedienteId == expedienteId.Value)
            );
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();

            query = query.Where(x =>
                EF.Functions.ILike(x.Titulo, $"%{termino}%")
                || (
                    x.Descripcion != null
                    && EF.Functions.ILike(x.Descripcion, $"%{termino}%")
                )
                || x.Clientes.Any(r =>
                    EF.Functions.ILike(r.Cliente.Nombre, $"%{termino}%")
                    || EF.Functions.ILike(r.Cliente.Apellido, $"%{termino}%")
                )
                || x.Casos.Any(r =>
                    EF.Functions.ILike(r.Caso.Titulo, $"%{termino}%")
                )
                || x.Expedientes.Any(r =>
                    EF.Functions.ILike(r.Expediente.Caratula, $"%{termino}%")
                    || (
                        r.Expediente.NumeroExpediente != null
                        && EF.Functions.ILike(
                            r.Expediente.NumeroExpediente,
                            $"%{termino}%"
                        )
                    )
                )
            );
        }

        return query;
    }
}
