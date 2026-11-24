namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;

public sealed class ConfirmarImportacionFeriadosRequest
{
    // Huella devuelta por la vista previa.
    public string Huella { get; set; } = string.Empty;

    // Fechas de "requierenRevision" que el usuario decide importar igual.
    public DateOnly[] FechasRevisionIncluidas { get; set; } = Array.Empty<DateOnly>();
}
