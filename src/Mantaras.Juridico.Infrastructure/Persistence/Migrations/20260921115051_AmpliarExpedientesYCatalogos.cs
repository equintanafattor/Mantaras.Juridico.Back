using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarExpedientesYCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expedientes_Casos_CasoId",
                table: "Expedientes");

            migrationBuilder.DropIndex(
                name: "IX_Expedientes_CasoId",
                table: "Expedientes");

            migrationBuilder.DropIndex(
                name: "IX_Expedientes_CasoId_Principal",
                table: "Expedientes");

            migrationBuilder.AlterColumn<string>(
                name: "FaseInterna",
                table: "Casos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "NumeroBeneficio",
                table: "Casos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CasosExpedientes",
                columns: table => new
                {
                    CasoId = table.Column<long>(type: "bigint", nullable: false),
                    ExpedienteId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CasosExpedientes", x => new { x.CasoId, x.ExpedienteId });
                    table.ForeignKey(
                        name: "FK_CasosExpedientes_Casos_CasoId",
                        column: x => x.CasoId,
                        principalTable: "Casos",
                        principalColumn: "CasoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CasosExpedientes_Expedientes_ExpedienteId",
                        column: x => x.ExpedienteId,
                        principalTable: "Expedientes",
                        principalColumn: "ExpedienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO "CasosExpedientes" ("CasoId", "ExpedienteId")
                SELECT "CasoId", "ExpedienteId"
                FROM "Expedientes";
                """
            );

            migrationBuilder.DropColumn(
                name: "CasoId",
                table: "Expedientes");

            migrationBuilder.CreateTable(
                name: "OpcionesCatalogo",
                columns: table => new
                {
                    OpcionCatalogoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcionesCatalogo", x => x.OpcionCatalogoId);
                });

            migrationBuilder.InsertData(
                table: "OpcionesCatalogo",
                columns: new[] { "OpcionCatalogoId", "Activo", "FechaCreacion", "FechaModificacion", "Nombre", "Tipo", "UsuarioCreacion", "UsuarioModificacion" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Preadministrativa", "fases-internas", "Sistema", null },
                    { 2L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Juicio", "fases-internas", "Sistema", null },
                    { 3L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Postjuicio", "fases-internas", "Sistema", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OpcionesCatalogo_Tipo_Nombre",
                table: "OpcionesCatalogo",
                columns: new[] { "Tipo", "Nombre" },
                unique: true);

            migrationBuilder.Sql(
                """
                SELECT setval(
                    pg_get_serial_sequence('"OpcionesCatalogo"', 'OpcionCatalogoId'),
                    (SELECT MAX("OpcionCatalogoId") FROM "OpcionesCatalogo"),
                    true
                );

                INSERT INTO "OpcionesCatalogo"
                    ("Tipo", "Nombre", "FechaCreacion", "UsuarioCreacion", "Activo")
                SELECT
                    'fases-internas',
                    TRIM("FaseInterna"),
                    NOW(),
                    'Migracion',
                    TRUE
                FROM "Casos"
                WHERE NULLIF(TRIM("FaseInterna"), '') IS NOT NULL
                GROUP BY TRIM("FaseInterna")
                ON CONFLICT ("Tipo", "Nombre") DO NOTHING;

                INSERT INTO "OpcionesCatalogo"
                    ("Tipo", "Nombre", "FechaCreacion", "UsuarioCreacion", "Activo")
                SELECT
                    'tipos-tramite',
                    TRIM("TipoTramite"),
                    NOW(),
                    'Migracion',
                    TRUE
                FROM "Casos"
                WHERE NULLIF(TRIM("TipoTramite"), '') IS NOT NULL
                GROUP BY TRIM("TipoTramite")
                ON CONFLICT ("Tipo", "Nombre") DO NOTHING;

                INSERT INTO "OpcionesCatalogo"
                    ("Tipo", "Nombre", "FechaCreacion", "UsuarioCreacion", "Activo")
                SELECT
                    'estados-legales',
                    TRIM("EstadoLegal"),
                    NOW(),
                    'Migracion',
                    TRUE
                FROM "Expedientes"
                WHERE NULLIF(TRIM("EstadoLegal"), '') IS NOT NULL
                GROUP BY TRIM("EstadoLegal")
                ON CONFLICT ("Tipo", "Nombre") DO NOTHING;
                """
            );

            migrationBuilder.CreateIndex(
                name: "IX_Casos_NumeroBeneficio",
                table: "Casos",
                column: "NumeroBeneficio");

            migrationBuilder.CreateIndex(
                name: "IX_CasosExpedientes_ExpedienteId",
                table: "CasosExpedientes",
                column: "ExpedienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OpcionesCatalogo_Tipo_Activo",
                table: "OpcionesCatalogo",
                columns: new[] { "Tipo", "Activo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CasoId",
                table: "Expedientes",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Expedientes" AS e
                SET "CasoId" = relaciones."CasoId"
                FROM (
                    SELECT "ExpedienteId", MIN("CasoId") AS "CasoId"
                    FROM "CasosExpedientes"
                    GROUP BY "ExpedienteId"
                ) AS relaciones
                WHERE e."ExpedienteId" = relaciones."ExpedienteId";
                """
            );

            migrationBuilder.AlterColumn<long>(
                name: "CasoId",
                table: "Expedientes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "CasosExpedientes");

            migrationBuilder.DropTable(
                name: "OpcionesCatalogo");

            migrationBuilder.DropIndex(
                name: "IX_Casos_NumeroBeneficio",
                table: "Casos");

            migrationBuilder.DropColumn(
                name: "NumeroBeneficio",
                table: "Casos");

            migrationBuilder.AlterColumn<string>(
                name: "FaseInterna",
                table: "Casos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_CasoId",
                table: "Expedientes",
                column: "CasoId");

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_CasoId_Principal",
                table: "Expedientes",
                column: "CasoId",
                unique: true,
                filter: "\"TipoExpediente\" = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_Expedientes_Casos_CasoId",
                table: "Expedientes",
                column: "CasoId",
                principalTable: "Casos",
                principalColumn: "CasoId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
