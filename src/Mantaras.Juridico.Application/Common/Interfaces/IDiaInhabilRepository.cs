using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface IDiaInhabilRepository
{
    Task<DiaInhabil?> ObtenerPorIdAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> ExisteFechaAsync(
        DateOnly fecha,
        long? idExcluir = null,
        CancellationToken cancellationToken = default
    );

    Task AgregarAsync(DiaInhabil entidad, CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DiaInhabil>> BuscarAsync(
        int? anio,
        bool soloActivos,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<int> ContarAsync(
        int? anio,
        bool soloActivos,
        CancellationToken cancellationToken = default
    );

    // Con seguimiento: registros con Fecha o FechaFuente en el año, activos o no.
    Task<IReadOnlyCollection<DiaInhabil>> ObtenerParaImportacionAsync(
        int anio,
        CancellationToken cancellationToken = default
    );

    Task<CalendarioFeriadosImportado?> ObtenerCalendarioAsync(
        int anio,
        bool seguimiento,
        CancellationToken cancellationToken = default
    );

    Task AgregarCalendarioAsync(
        CalendarioFeriadosImportado calendario,
        CancellationToken cancellationToken = default
    );

    Task<(int Total, int Importados, int Manuales)> ContarActivosPorOrigenAsync(
        int anio,
        CancellationToken cancellationToken = default
    );

    // Abre una transacción y toma un bloqueo exclusivo por año: dos confirmaciones
    // del mismo año se ejecutan una después de la otra. Sin ConfirmarAsync, se revierte.
    Task<ITransaccionImportacionFeriados> IniciarImportacionExclusivaAsync(
        int anio,
        CancellationToken cancellationToken = default
    );

    // Guarda la importación entera en un único SaveChanges. Una violación de
    // unicidad (fecha o año) se informa como RegistroConcurrenteException.
    Task GuardarImportacionAsync(CancellationToken cancellationToken = default);

    void DescartarCambios();
}

public interface ITransaccionImportacionFeriados : IAsyncDisposable
{
    Task ConfirmarAsync(CancellationToken cancellationToken = default);
}
