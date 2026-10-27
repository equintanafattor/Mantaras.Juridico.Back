using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSeguimientoCobroJudicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "ReglasVencimiento",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EnviosCobroExpedientes",
                columns: table => new
                {
                    EnvioCobroExpedienteId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExpedienteId = table.Column<long>(type: "bigint", nullable: false),
                    TipoDocumento = table.Column<int>(type: "integer", nullable: false),
                    FechaEnvio = table.Column<DateOnly>(type: "date", nullable: false),
                    AgendaGeneracionReglaId = table.Column<long>(type: "bigint", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnviosCobroExpedientes", x => x.EnvioCobroExpedienteId);
                    table.ForeignKey(
                        name: "FK_EnviosCobroExpedientes_AgendaGeneracionesReglas_AgendaGener~",
                        column: x => x.AgendaGeneracionReglaId,
                        principalTable: "AgendaGeneracionesReglas",
                        principalColumn: "AgendaGeneracionReglaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnviosCobroExpedientes_Expedientes_ExpedienteId",
                        column: x => x.ExpedienteId,
                        principalTable: "Expedientes",
                        principalColumn: "ExpedienteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReglasVencimiento_Codigo",
                table: "ReglasVencimiento",
                column: "Codigo",
                unique: true,
                filter: "\"Codigo\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EnviosCobroExpedientes_AgendaGeneracionReglaId",
                table: "EnviosCobroExpedientes",
                column: "AgendaGeneracionReglaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EnviosCobroExpedientes_ExpedienteId",
                table: "EnviosCobroExpedientes",
                column: "ExpedienteId",
                unique: true,
                filter: "\"Activo\"");

            // Siembra idempotente sin identificadores explícitos para no colisionar
            // con tipos o reglas cargados por los usuarios.
            migrationBuilder.Sql(
                """
                INSERT INTO "TiposEntradaAgenda"
                    ("Nombre", "Descripcion", "Color", "FechaCreacion", "UsuarioCreacion", "Activo")
                VALUES (
                    'Seguimiento de cobro',
                    'Seguimiento interno de cédulas u oficios de cobro enviados desde el expediente judicial.',
                    'orange',
                    TIMESTAMPTZ '2026-10-27 00:00:00+00',
                    'Sistema',
                    TRUE
                )
                ON CONFLICT ("Nombre") DO NOTHING;

                INSERT INTO "ReglasVencimiento"
                    ("Codigo", "TipoEntradaAgendaId", "Nombre", "Descripcion", "CantidadDias",
                     "TipoComputo", "SentidoCalculo", "PrioridadGenerada", "HoraSugerida",
                     "FechaCreacion", "UsuarioCreacion", "Activo")
                SELECT
                    'SEGUIMIENTO_COBRO_JUDICIAL',
                    t."TipoEntradaAgendaId",
                    'Seguimiento de cobro judicial',
                    'Alerta interna a los 120 días hábiles desde el envío de la cédula u oficio de cobro.',
                    120,
                    2,
                    1,
                    2,
                    NULL,
                    TIMESTAMPTZ '2026-10-27 00:00:00+00',
                    'Sistema',
                    TRUE
                FROM "TiposEntradaAgenda" t
                WHERE t."Nombre" = 'Seguimiento de cobro'
                ON CONFLICT DO NOTHING;

                INSERT INTO "RecordatoriosPredeterminadosReglasVencimiento"
                    ("ReglaVencimientoId", "BaseCalculo", "MinutosAnticipacion",
                     "FechaCreacion", "UsuarioCreacion", "Activo")
                SELECT
                    r."ReglaVencimientoId",
                    2,
                    0,
                    TIMESTAMPTZ '2026-10-27 00:00:00+00',
                    'Sistema',
                    TRUE
                FROM "ReglasVencimiento" r
                WHERE r."Codigo" = 'SEGUIMIENTO_COBRO_JUDICIAL'
                ON CONFLICT DO NOTHING;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnviosCobroExpedientes");

            // Solo se retira la regla sembrada si nunca generó entradas.
            migrationBuilder.Sql(
                """
                DELETE FROM "ReglasVencimiento" r
                WHERE r."Codigo" = 'SEGUIMIENTO_COBRO_JUDICIAL'
                  AND r."UsuarioCreacion" = 'Sistema'
                  AND NOT EXISTS (
                      SELECT 1 FROM "AgendaGeneracionesReglas" g
                      WHERE g."ReglaVencimientoId" = r."ReglaVencimientoId"
                  );
                """
            );

            migrationBuilder.DropIndex(
                name: "IX_ReglasVencimiento_Codigo",
                table: "ReglasVencimiento");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "ReglasVencimiento");
        }
    }
}
