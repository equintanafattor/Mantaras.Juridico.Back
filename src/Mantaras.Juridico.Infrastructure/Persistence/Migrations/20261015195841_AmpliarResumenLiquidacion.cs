using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarResumenLiquidacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaAdquisicionDerecho",
                table: "HojasResumenCasos",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeyAplicada",
                table: "HojasResumenCasos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionesGenerales",
                table: "HojasResumenCasos",
                type: "character varying(6500)",
                maxLength: 6500,
                nullable: true);
            // Unificar sin descartar ni truncar las observaciones históricas.
            migrationBuilder.Sql(
                """
                UPDATE "HojasResumenCasos"
                SET "ObservacionesGenerales" = NULLIF(CONCAT_WS(E'\n\n',
                    CASE WHEN NULLIF(BTRIM("HaberInicialObservacion"), '') IS NOT NULL
                        THEN 'Haber inicial: ' || "HaberInicialObservacion" END,
                    CASE WHEN NULLIF(BTRIM("MovilidadObservaciones"), '') IS NOT NULL
                        THEN 'Movilidad: ' || "MovilidadObservaciones" END,
                    CASE WHEN NULLIF(BTRIM("RetroactivoObservacion"), '') IS NOT NULL
                        THEN 'Retroactivo: ' || "RetroactivoObservacion" END
                ), '')
                WHERE "ObservacionesGenerales" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaAdquisicionDerecho",
                table: "HojasResumenCasos");

            migrationBuilder.DropColumn(
                name: "LeyAplicada",
                table: "HojasResumenCasos");

            migrationBuilder.DropColumn(
                name: "ObservacionesGenerales",
                table: "HojasResumenCasos");
        }
    }
}
