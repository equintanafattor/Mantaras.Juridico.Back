using Mantaras.Juridico.Domain.Common;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class EntradaAgenda : AuditableEntity
{
    public const string ZonaHorariaPredeterminada = "America/Argentina/Buenos_Aires";

    public long EntradaAgendaId { get; set; }

    public long TipoEntradaAgendaId { get; set; }

    public long? RecurrenciaAgendaId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public EstadoEntradaAgenda Estado { get; set; } = EstadoEntradaAgenda.Pendiente;

    public PrioridadAgenda Prioridad { get; set; } = PrioridadAgenda.Normal;

    public DateOnly FechaInicio { get; set; }

    public TimeOnly? HoraInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public TimeOnly? HoraFin { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    public TimeOnly? HoraVencimiento { get; set; }

    public string ZonaHoraria { get; set; } = ZonaHorariaPredeterminada;

    public TipoEntradaAgenda TipoEntrada { get; set; } = null!;

    public RecurrenciaAgenda? Recurrencia { get; set; }

    public AgendaGeneracionRegla? GeneracionRegla { get; set; }

    public ICollection<EntradaAgendaCliente> Clientes { get; set; } =
        new List<EntradaAgendaCliente>();

    public ICollection<EntradaAgendaCaso> Casos { get; set; } =
        new List<EntradaAgendaCaso>();

    public ICollection<EntradaAgendaExpediente> Expedientes { get; set; } =
        new List<EntradaAgendaExpediente>();

    public ICollection<EntradaAgendaResponsable> Responsables { get; set; } =
        new List<EntradaAgendaResponsable>();

    public ICollection<RecordatorioAgenda> Recordatorios { get; set; } =
        new List<RecordatorioAgenda>();

    public ICollection<AgendaGeneracionRegla> GeneracionesDependientes { get; set; } =
        new List<AgendaGeneracionRegla>();
}
