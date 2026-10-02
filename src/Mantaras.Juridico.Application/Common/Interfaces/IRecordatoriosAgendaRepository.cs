using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IRecordatoriosAgendaRepository
{
    Task<RecordatorioAgenda?> ObtenerPorIdAsync(
        long recordatorioAgendaId,
        bool seguimiento = true,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExisteAsync(
        long entradaAgendaId,
        DateTime fechaProgramadaUtc,
        CancellationToken cancellationToken = default,
        long? recordatorioExcluirId = null
    );

    Task<IReadOnlyCollection<RecordatorioAgenda>> BuscarAsync(
        long? entradaAgendaId,
        EstadoRecordatorioAgenda? estado,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<int> ContarAsync(
        long? entradaAgendaId,
        EstadoRecordatorioAgenda? estado,
        CancellationToken cancellationToken = default
    );

    Task AgregarAsync(
        RecordatorioAgenda recordatorio,
        CancellationToken cancellationToken = default
    );

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default
    );
}
