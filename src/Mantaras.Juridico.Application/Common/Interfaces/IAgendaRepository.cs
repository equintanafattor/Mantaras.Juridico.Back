using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IAgendaRepository
{
    Task<EntradaAgenda?> ObtenerPorIdAsync(
        long entradaAgendaId,
        bool seguimiento = true,
        CancellationToken cancellationToken = default
    );

    Task<TipoEntradaAgenda?> ObtenerTipoPorIdAsync(
        long tipoEntradaAgendaId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistenClientesActivosAsync(
        IReadOnlyCollection<long> clienteIds,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistenCasosActivosAsync(
        IReadOnlyCollection<long> casoIds,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistenExpedientesActivosAsync(
        IReadOnlyCollection<long> expedienteIds,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistenUsuariosActivosAsync(
        IReadOnlyCollection<long> usuarioIds,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<EntradaAgenda>> BuscarAsync(
        DateOnly? desde,
        DateOnly? hasta,
        long? tipoEntradaAgendaId,
        EstadoEntradaAgenda? estado,
        long? responsableId,
        long? clienteId,
        long? casoId,
        long? expedienteId,
        string? busqueda,
        bool incluirVencimientos,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<int> ContarAsync(
        DateOnly? desde,
        DateOnly? hasta,
        long? tipoEntradaAgendaId,
        EstadoEntradaAgenda? estado,
        long? responsableId,
        long? clienteId,
        long? casoId,
        long? expedienteId,
        string? busqueda,
        bool incluirVencimientos,
        bool soloActivos,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyDictionary<long, string>> ObtenerNombresUsuariosAsync(
        IReadOnlyCollection<long> usuarioIds,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ReglaVencimiento>> ObtenerReglasAsync(
        bool soloActivas,
        CancellationToken cancellationToken = default
    );

    Task<ReglaVencimiento?> ObtenerReglaPorIdAsync(
        long reglaVencimientoId,
        bool seguimiento = true,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExisteNombreReglaAsync(
        string nombre,
        long? reglaVencimientoIdExcluir = null,
        CancellationToken cancellationToken = default
    );

    Task<AgendaGeneracionRegla?> ObtenerGeneracionPorClaveAsync(
        string claveIdempotencia,
        CancellationToken cancellationToken = default
    );

    Task<Observacion?> ObtenerObservacionPorIdAsync(
        long observacionId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<DateOnly>> ObtenerDiasInhabilesActivosAsync(
        CancellationToken cancellationToken = default
    );

    Task AgregarReglaAsync(
        ReglaVencimiento regla,
        CancellationToken cancellationToken = default
    );

    Task AgregarGeneracionAsync(
        AgendaGeneracionRegla generacion,
        CancellationToken cancellationToken = default
    );

    Task AgregarAsync(
        EntradaAgenda entrada,
        CancellationToken cancellationToken = default
    );

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default
    );
}
