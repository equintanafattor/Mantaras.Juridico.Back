using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Responses;

public sealed class EnvioCobroResponse
{
    public long EnvioCobroExpedienteId { get; set; }

    public TipoDocumentoCobro TipoDocumento { get; set; }

    public DateOnly FechaEnvio { get; set; }

    public long AgendaGeneracionReglaId { get; set; }

    public long ReglaVencimientoId { get; set; }

    public long EntradaAgendaId { get; set; }

    public DateOnly FechaSeguimiento { get; set; }

    public EstadoEntradaAgenda EstadoSeguimiento { get; set; }

    // Días corridos (no hábiles) desde hoy hasta FechaSeguimiento. Negativo si ya pasó.
    // Los 120 días hábiles se usan para calcular FechaSeguimiento, no para este contador.
    public int DiasParaSeguimiento { get; set; }

    // true solo si el seguimiento sigue abierto (no Completada/Cancelada) y FechaSeguimiento ya pasó.
    public bool SeguimientoVencido { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string? UsuarioRegistro { get; set; }
}
