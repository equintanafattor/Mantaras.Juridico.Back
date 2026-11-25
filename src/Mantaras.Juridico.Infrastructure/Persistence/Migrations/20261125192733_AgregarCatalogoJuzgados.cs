using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCatalogoJuzgados : Migration
    {
        // Replica en SQL NombreCatalogo.Normalizar (C#): colapsa cualquier espacio en
        // blanco Unicode (los mismos que char.IsWhiteSpace), recorta y pasa a mayúsculas.
        // translate() asegura las vocales acentuadas, la Ñ y la Ç aunque el locale de
        // la base no las convierta en upper() (por ejemplo, LC_CTYPE=C).
        internal static string NormalizarSql(string columna) =>
            $"""
            translate(
                upper(btrim(regexp_replace(
                    {columna},
                    '[' || chr(9) || '-' || chr(13) || ' ' || chr(133) || chr(160) || chr(5760)
                        || chr(8192) || '-' || chr(8202) || chr(8232) || chr(8233) || chr(8239)
                        || chr(8287) || chr(12288) || ']+',
                    ' ',
                    'g'
                ))),
                'áéíóúüñàèìòùâêîôûäëïöç',
                'ÁÉÍÓÚÜÑÀÈÌÒÙÂÊÎÔÛÄËÏÖÇ'
            )
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Juzgados",
                columns: table => new
                {
                    JuzgadoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Juzgados", x => x.JuzgadoId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Juzgados_Nombre",
                table: "Juzgados",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddColumn<long>(
                name: "JuzgadoId",
                table: "Expedientes",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_JuzgadoId",
                table: "Expedientes",
                column: "JuzgadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expedientes_Juzgados_JuzgadoId",
                table: "Expedientes",
                column: "JuzgadoId",
                principalTable: "Juzgados",
                principalColumn: "JuzgadoId",
                onDelete: ReferentialAction.Restrict);

            var normalizado = NormalizarSql("e.\"Juzgado\"");

            // 1. Un juzgado activo por cada nombre distinto una vez normalizado:
            //    las variantes de mayúsculas y espacios quedan unificadas.
            //    Los textos vacíos o solo con espacios no generan juzgado.
            migrationBuilder.Sql(
                $"""
                INSERT INTO "Juzgados" ("Nombre", "FechaCreacion", "UsuarioCreacion", "Activo")
                SELECT n.nombre, CURRENT_TIMESTAMP, 'Sistema', TRUE
                FROM (
                    SELECT DISTINCT {normalizado} AS nombre
                    FROM "Expedientes" e
                    WHERE e."Juzgado" IS NOT NULL
                ) n
                WHERE n.nombre <> ''
                ORDER BY n.nombre;
                """
            );

            // 2. Vincula cada expediente (activo o dado de baja) con su juzgado.
            migrationBuilder.Sql(
                $"""
                UPDATE "Expedientes" e
                SET "JuzgadoId" = j."JuzgadoId"
                FROM "Juzgados" j
                WHERE e."Juzgado" IS NOT NULL
                  AND j."Nombre" = {normalizado};
                """
            );

            // 3. Resguardo: no se retira la columna si algún texto quedó sin vincular.
            //    La migración corre en una transacción, así que un error la revierte entera.
            migrationBuilder.Sql(
                $"""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM "Expedientes" e
                        WHERE e."JuzgadoId" IS NULL
                          AND e."Juzgado" IS NOT NULL
                          AND {normalizado} <> ''
                    ) THEN
                        RAISE EXCEPTION 'AgregarCatalogoJuzgados: hay expedientes con juzgado sin vincular; no se retira la columna Juzgado.';
                    END IF;
                END
                $$;
                """
            );

            migrationBuilder.DropColumn(
                name: "Juzgado",
                table: "Expedientes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Juzgado",
                table: "Expedientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            // Restituye el texto con el nombre del catálogo (normalizado en mayúsculas):
            // las variantes de escritura originales unificadas no se recuperan.
            migrationBuilder.Sql(
                """
                UPDATE "Expedientes" e
                SET "Juzgado" = j."Nombre"
                FROM "Juzgados" j
                WHERE e."JuzgadoId" = j."JuzgadoId";
                """
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Expedientes_Juzgados_JuzgadoId",
                table: "Expedientes");

            migrationBuilder.DropIndex(
                name: "IX_Expedientes_JuzgadoId",
                table: "Expedientes");

            migrationBuilder.DropColumn(
                name: "JuzgadoId",
                table: "Expedientes");

            migrationBuilder.DropTable(
                name: "Juzgados");
        }
    }
}
