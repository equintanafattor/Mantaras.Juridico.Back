using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Responses;

public sealed class ExpedienteRelacionadoResponse
{
    public long ExpedienteId { get; set; }

    public TipoExpediente TipoExpediente { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public bool Activo { get; set; }
}