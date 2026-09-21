namespace Mantaras.Juridico.Application.Features.Expedientes.Responses;

public sealed class CasoExpedienteResponse
{
    public long CasoId { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? NumeroExpedienteAnses { get; set; }

    public string? NumeroBeneficio { get; set; }

    public long? TipoBeneficioId { get; set; }

    public string? TipoBeneficioNombre { get; set; }

    public bool? TipoBeneficioActivo { get; set; }

    public bool Activo { get; set; }
}
