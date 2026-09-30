using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Mantaras.Juridico.Infrastructure.Persistence.Repositories;

public sealed class RecordatoriosAgendaRepository
    : IRecordatoriosAgendaRepository
{
    private readonly JuridicoDbContext _dbContext;

    public RecordatoriosAgendaRepository(JuridicoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RecordatorioAgenda?> ObtenerPorIdAsync(
        long recordatorioAgendaId,
        bool seguimiento = true,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.RecordatoriosAgenda
            .Include(x => x.EntradaAgenda)
                .ThenInclude(x => x.TipoEntrada)
            .AsQueryable();

        if (!seguimiento)
        {
            query = query.AsNoTracking();
        }

        return query.FirstOrDefaultAsync(
            x =>
                x.RecordatorioAgendaId == recordatorioAgendaId
                && x.Activo,
            cancellationToken
        );
    }

    public Task<bool> ExisteAsync(
        long entradaAgendaId,
        DateTime fechaProgramadaUtc,
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.RecordatoriosAgenda.AnyAsync(
            x =>
                x.EntradaAgendaId == entradaAgendaId
                && x.FechaProgramadaUtc == fechaProgramadaUtc
                && x.Activo,
            cancellationToken
        );
    }

    public async Task<IReadOnlyCollection<RecordatorioAgenda>> BuscarAsync(
        long? entradaAgendaId,
        EstadoRecordatorioAgenda? estado,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        return await ConstruirConsulta(entradaAgendaId, estado)
            .Include(x => x.EntradaAgenda)
                .ThenInclude(x => x.TipoEntrada)
            .OrderBy(x => x.Atendido)
            .ThenBy(x => x.FechaProgramadaUtc)
            .ThenBy(x => x.RecordatorioAgendaId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> ContarAsync(
        long? entradaAgendaId,
        EstadoRecordatorioAgenda? estado,
        CancellationToken cancellationToken = default
    )
    {
        return ConstruirConsulta(entradaAgendaId, estado)
            .CountAsync(cancellationToken);
    }

    public async Task AgregarAsync(
        RecordatorioAgenda recordatorio,
        CancellationToken cancellationToken = default
    )
    {
        await _dbContext.RecordatoriosAgenda.AddAsync(
            recordatorio,
            cancellationToken
        );
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default
    )
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<RecordatorioAgenda> ConstruirConsulta(
        long? entradaAgendaId,
        EstadoRecordatorioAgenda? estado
    )
    {
        var ahora = DateTime.UtcNow;
        var query = _dbContext.RecordatoriosAgenda
            .Where(x => x.Activo && x.EntradaAgenda.Activo);

        if (entradaAgendaId.HasValue)
        {
            query = query.Where(x =>
                x.EntradaAgendaId == entradaAgendaId.Value
            );
        }

        query = estado switch
        {
            EstadoRecordatorioAgenda.Pendiente => query.Where(x =>
                !x.Atendido && x.FechaProgramadaUtc > ahora
            ),
            EstadoRecordatorioAgenda.Vencido => query.Where(x =>
                !x.Atendido && x.FechaProgramadaUtc <= ahora
            ),
            EstadoRecordatorioAgenda.Atendido => query.Where(x => x.Atendido),
            _ => query,
        };

        return query;
    }
}
