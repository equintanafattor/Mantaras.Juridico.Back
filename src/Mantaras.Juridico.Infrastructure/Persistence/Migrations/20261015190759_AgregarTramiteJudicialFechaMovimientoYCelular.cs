using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTramiteJudicialFechaMovimientoYCelular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaMovimiento",
                table: "Observaciones",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoTramite",
                table: "Expedientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Celular",
                table: "Clientes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // Los registros previos sólo conocen su fecha de carga UTC.
            // Se conserva esa auditoría y se inicializa la fecha del movimiento
            // con el día correspondiente en Argentina. Las notas de clientes
            // siguen sin fecha de movimiento.
            migrationBuilder.Sql(
                """
                UPDATE "Observaciones"
                SET "FechaMovimiento" =
                    ("FechaCreacion" AT TIME ZONE 'America/Argentina/Buenos_Aires')::date
                WHERE "FechaMovimiento" IS NULL
                  AND ("CasoId" IS NOT NULL OR "ExpedienteId" IS NOT NULL);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_CasoId_FechaMovimiento_FechaCreacion",
                table: "Observaciones",
                columns: new[] { "CasoId", "FechaMovimiento", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_ExpedienteId_FechaMovimiento_FechaCreacion",
                table: "Observaciones",
                columns: new[] { "ExpedienteId", "FechaMovimiento", "FechaCreacion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Observaciones_CasoId_FechaMovimiento_FechaCreacion",
                table: "Observaciones");

            migrationBuilder.DropIndex(
                name: "IX_Observaciones_ExpedienteId_FechaMovimiento_FechaCreacion",
                table: "Observaciones");

            migrationBuilder.DropColumn(
                name: "FechaMovimiento",
                table: "Observaciones");

            migrationBuilder.DropColumn(
                name: "TipoTramite",
                table: "Expedientes");

            migrationBuilder.DropColumn(
                name: "Celular",
                table: "Clientes");
        }
    }
}
