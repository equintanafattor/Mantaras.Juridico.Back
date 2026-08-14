namespace Mantaras.Juridico.Application.Features.Casos.Requests;

public sealed class CrearCasoConExpedientePrincipalRequest
{
    public CrearCasoRequest Caso { get; set; } = new();

    public CrearExpedientePrincipalRequest Expediente { get; set; } = new();
}