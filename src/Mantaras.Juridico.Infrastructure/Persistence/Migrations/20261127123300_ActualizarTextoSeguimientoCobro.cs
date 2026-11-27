using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Cambia "Alerta interna…" por "Aviso interno…" en el texto automático del seguimiento de cobro.
    ///
    /// El texto de cada seguimiento nuevo sale de la descripción de la regla SEGUIMIENTO_COBRO_JUDICIAL
    /// (sembrada por AgregarSeguimientoCobroJudicial) y se copia a la entrada de agenda al generarla.
    /// Solo se actualizan los textos idénticos al original:
    /// - la descripción de esa regla, si nadie la modificó;
    /// - la descripción de las entradas generadas por esa regla (AgendaGeneracionesReglas).
    /// Las notas editadas, las de otras reglas y las entradas manuales no se tocan. No cambia fechas,
    /// estados ni campos de auditoría.
    /// </summary>
    public partial class ActualizarTextoSeguimientoCobro : Migration
    {
        public const string CodigoRegla = "SEGUIMIENTO_COBRO_JUDICIAL";

        public const string TextoAnterior =
            "Alerta interna a los 120 días hábiles desde el envío de la cédula u oficio de cobro.";

        public const string TextoNuevo =
            "Aviso interno a los 120 días hábiles desde el envío de la cédula u oficio de cobro.";

        public static string Reemplazar(string desde, string hacia) =>
            $"""
            UPDATE "ReglasVencimiento"
            SET "Descripcion" = '{hacia}'
            WHERE "Codigo" = '{CodigoRegla}'
              AND "Descripcion" = '{desde}';

            UPDATE "EntradasAgenda" e
            SET "Descripcion" = '{hacia}'
            FROM "AgendaGeneracionesReglas" g
            JOIN "ReglasVencimiento" r ON r."ReglaVencimientoId" = g."ReglaVencimientoId"
            WHERE g."EntradaAgendaId" = e."EntradaAgendaId"
              AND r."Codigo" = '{CodigoRegla}'
              AND e."Descripcion" = '{desde}';
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Reemplazar(TextoAnterior, TextoNuevo));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Vuelve al texto anterior solo donde quedó el texto nuevo exacto.
            migrationBuilder.Sql(Reemplazar(TextoNuevo, TextoAnterior));
        }
    }
}
