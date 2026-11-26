using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.Expedientes.Responses;

public sealed class ExpedienteCobroResponse
{
    public long ExpedienteId { get; set; }

    public IReadOnlyCollection<CasoExpedienteResponse> Casos { get; set; } =
        Array.Empty<CasoExpedienteResponse>();

    // Clientes distintos de todos los casos vinculados. Nunca null.
    public IReadOnlyCollection<ClienteExpedienteCobroResponse> Clientes { get; set; } =
        Array.Empty<ClienteExpedienteCobroResponse>();

    // Null cuando no hay un principal inequívoco (ver ClientesExpedienteCobro).
    public long? ClientePrincipalId { get; set; }

    public TipoExpediente TipoExpediente { get; set; }

    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public long? JuzgadoId { get; set; }

    public string? JuzgadoNombre { get; set; }

    public bool? JuzgadoActivo { get; set; }

    public string? EstadoLegal { get; set; }

    public bool Activo { get; set; }

    public EnvioCobroResponse EnvioCobro { get; set; } = null!;
}
