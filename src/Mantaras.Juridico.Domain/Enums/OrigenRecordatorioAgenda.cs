namespace Mantaras.Juridico.Domain.Enums;

// Procedencia de un aviso (recordatorio de agenda), registrada al crearlo.
public enum OrigenRecordatorioAgenda
{
    // Avisos anteriores al registro del origen: no hay evidencia confiable de su procedencia.
    Desconocido = 0,

    // Agregado a mano (POST /api/recordatorios-agenda/entrada/{id}).
    Manual = 1,

    // Aviso predeterminado de la categoría (TipoEntradaAgenda) de la entrada.
    Categoria = 2,

    // Aviso predeterminado de la regla de vencimiento que generó la entrada
    // (incluye el seguimiento de cobro judicial).
    Regla = 3,
}
