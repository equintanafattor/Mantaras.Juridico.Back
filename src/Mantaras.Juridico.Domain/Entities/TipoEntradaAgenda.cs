using Mantaras.Juridico.Domain.Common;

namespace Mantaras.Juridico.Domain.Entities;

public sealed class TipoEntradaAgenda : AuditableEntity
{
    public long TipoEntradaAgendaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string? Color { get; set; }

    public ICollection<EntradaAgenda> Entradas { get; set; } =
        new List<EntradaAgenda>();

    public ICollection<RecordatorioPredeterminadoTipoAgenda> RecordatoriosPredeterminados
    {
        get;
        set;
    } = new List<RecordatorioPredeterminadoTipoAgenda>();

    public ICollection<ReglaVencimiento> ReglasVencimiento { get; set; } =
        new List<ReglaVencimiento>();
}
