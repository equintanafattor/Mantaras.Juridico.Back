using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyObservacionesColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            """
            INSERT INTO "Observaciones"
                ("Texto", "ClienteId", "FechaCreacion", "UsuarioCreacion")
            SELECT
                BTRIM(cliente."Observaciones"),
                cliente."ClienteId",
                COALESCE(
                    cliente."FechaModificacion",
                    cliente."FechaCreacion"
                ),
                COALESCE(
                    cliente."UsuarioModificacion",
                    cliente."UsuarioCreacion"
                )
            FROM "Clientes" AS cliente
            WHERE cliente."Observaciones" IS NOT NULL
            AND BTRIM(cliente."Observaciones") <> ''
            AND NOT EXISTS (
                SELECT 1
                FROM "Observaciones" AS observacion
                WHERE observacion."ClienteId" = cliente."ClienteId"
                    AND observacion."Texto" =
                        BTRIM(cliente."Observaciones")
            );

            INSERT INTO "Observaciones"
                ("Texto", "CasoId", "FechaCreacion", "UsuarioCreacion")
            SELECT
                BTRIM(caso."Observaciones"),
                caso."CasoId",
                COALESCE(
                    caso."FechaModificacion",
                    caso."FechaCreacion"
                ),
                COALESCE(
                    caso."UsuarioModificacion",
                    caso."UsuarioCreacion"
                )
            FROM "Casos" AS caso
            WHERE caso."Observaciones" IS NOT NULL
            AND BTRIM(caso."Observaciones") <> ''
            AND NOT EXISTS (
                SELECT 1
                FROM "Observaciones" AS observacion
                WHERE observacion."CasoId" = caso."CasoId"
                    AND observacion."Texto" =
                        BTRIM(caso."Observaciones")
            );

            INSERT INTO "Observaciones"
                ("Texto", "ExpedienteId", "FechaCreacion", "UsuarioCreacion")
            SELECT
                BTRIM(expediente."Observaciones"),
                expediente."ExpedienteId",
                COALESCE(
                    expediente."FechaModificacion",
                    expediente."FechaCreacion"
                ),
                COALESCE(
                    expediente."UsuarioModificacion",
                    expediente."UsuarioCreacion"
                )
            FROM "Expedientes" AS expediente
            WHERE expediente."Observaciones" IS NOT NULL
            AND BTRIM(expediente."Observaciones") <> ''
            AND NOT EXISTS (
                SELECT 1
                FROM "Observaciones" AS observacion
                WHERE observacion."ExpedienteId" =
                    expediente."ExpedienteId"
                    AND observacion."Texto" =
                        BTRIM(expediente."Observaciones")
            );
            """
        );

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Expedientes");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Casos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Expedientes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Clientes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Casos",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
