using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Requests;

public sealed class RegistrarEnvioCobroRequest
{
    public TipoDocumentoCobro TipoDocumento { get; set; }

    public DateOnly FechaEnvio { get; set; }
}
