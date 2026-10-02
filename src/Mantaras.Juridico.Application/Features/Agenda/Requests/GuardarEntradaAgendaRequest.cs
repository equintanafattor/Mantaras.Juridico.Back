using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Agenda.Requests;

public sealed class GuardarEntradaAgendaRequest
{
    public long TipoEntradaAgendaId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public PrioridadAgenda Prioridad { get; set; } = PrioridadAgenda.Normal;

    public DateOnly FechaInicio { get; set; }

    public TimeOnly? HoraInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public TimeOnly? HoraFin { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    public TimeOnly? HoraVencimiento { get; set; }

    public IReadOnlyCollection<long> ClienteIds { get; set; } = Array.Empty<long>();

    public IReadOnlyCollection<long> CasoIds { get; set; } = Array.Empty<long>();

    public IReadOnlyCollection<long> ExpedienteIds { get; set; } = Array.Empty<long>();

    public IReadOnlyCollection<long> ResponsableIds { get; set; } = Array.Empty<long>();

    // Sólo se permite al crear. La primera ocurrencia es la entrada solicitada.
    public RecurrenciaEntradaRequest? Recurrencia { get; set; }
}

public sealed class RecurrenciaEntradaRequest
{
    public FrecuenciaRecurrenciaAgenda Frecuencia { get; set; }
    public int Intervalo { get; set; } = 1;
    public int CantidadOcurrencias { get; set; }
}
