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

    public int DiasParaSeguimiento { get; set; }

    public bool SeguimientoVencido { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string? UsuarioRegistro { get; set; }
}
