namespace Mantaras.Juridico.Application.Features.Casos.Requests;

public sealed class CrearExpedientePrincipalRequest
{
    public string? NumeroExpediente { get; set; }

    public string Caratula { get; set; } = string.Empty;

    public string? Juzgado { get; set; }

    public DateOnly? FechaInicio { get; set; }

    public string? EstadoLegal { get; set; }

    public string? Observaciones { get; set; }
}