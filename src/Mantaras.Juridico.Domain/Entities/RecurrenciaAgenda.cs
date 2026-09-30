using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class RecurrenciaAgenda : AuditableEntity
{
    public long RecurrenciaAgendaId { get; set; }

    public FrecuenciaRecurrenciaAgenda Frecuencia { get; set; }

    public int Intervalo { get; set; } = 1;

    public int? DiasSemana { get; set; }

    public int? DiaMes { get; set; }

    public int? MesAnio { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public int? MaximoOcurrencias { get; set; }

    public int OcurrenciasGeneradas { get; set; }

    public DateOnly? ProximaFecha { get; set; }

    public string ZonaHoraria { get; set; } = EntradaAgenda.ZonaHorariaPredeterminada;

    public ICollection<EntradaAgenda> Entradas { get; set; } =
        new List<EntradaAgenda>();
}
