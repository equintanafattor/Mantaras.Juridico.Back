namespace Mantaras.Juridico.Domain.Entities;

public sealed class Observacion
{
    public long ObservacionId { get; set; }

    public string Texto { get; set; } = string.Empty;

    public long? ClienteId { get; set; }

    public long? CasoId { get; set; }

    public long? ExpedienteId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string? UsuarioCreacion { get; set; }

    public Cliente? Cliente { get; set; }

    public Caso? Caso { get; set; }

    public Expediente? Expediente { get; set; }
}