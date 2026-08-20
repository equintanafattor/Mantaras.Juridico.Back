using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHistorialObservaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Observaciones",
                columns: table => new
                {
                    ObservacionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: true),
                    CasoId = table.Column<long>(type: "bigint", nullable: true),
                    ExpedienteId = table.Column<long>(type: "bigint", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Observaciones", x => x.ObservacionId);
                    table.CheckConstraint("CK_Observaciones_UnPropietario", "num_nonnulls(\"ClienteId\", \"CasoId\", \"ExpedienteId\") = 1");
                    table.ForeignKey(
                        name: "FK_Observaciones_Casos_CasoId",
                        column: x => x.CasoId,
                        principalTable: "Casos",
                        principalColumn: "CasoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Observaciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Observaciones_Expedientes_ExpedienteId",
                        column: x => x.ExpedienteId,
                        principalTable: "Expedientes",
                        principalColumn: "ExpedienteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_CasoId_FechaCreacion",
                table: "Observaciones",
                columns: new[] { "CasoId", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_ClienteId_FechaCreacion",
                table: "Observaciones",
                columns: new[] { "ClienteId", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Observaciones_ExpedienteId_FechaCreacion",
                table: "Observaciones",
                columns: new[] { "ExpedienteId", "FechaCreacion" });
                
            migrationBuilder.Sql(
                """
                INSERT INTO "Observaciones"
                    ("Texto", "ClienteId", "FechaCreacion", "UsuarioCreacion")
                SELECT
                    BTRIM("Observaciones"),
                    "ClienteId",
                    COALESCE("FechaModificacion", "FechaCreacion"),
                    COALESCE("UsuarioModificacion", "UsuarioCreacion")
                FROM "Clientes"
                WHERE "Observaciones" IS NOT NULL
                AND BTRIM("Observaciones") <> '';

                INSERT INTO "Observaciones"
                    ("Texto", "CasoId", "FechaCreacion", "UsuarioCreacion")
                SELECT
                    BTRIM("Observaciones"),
                    "CasoId",
                    COALESCE("FechaModificacion", "FechaCreacion"),
                    COALESCE("UsuarioModificacion", "UsuarioCreacion")
                FROM "Casos"
                WHERE "Observaciones" IS NOT NULL
                AND BTRIM("Observaciones") <> '';

                INSERT INTO "Observaciones"
                    ("Texto", "ExpedienteId", "FechaCreacion", "UsuarioCreacion")
                SELECT
                    BTRIM("Observaciones"),
                    "ExpedienteId",
                    COALESCE("FechaModificacion", "FechaCreacion"),
                    COALESCE("UsuarioModificacion", "UsuarioCreacion")
                FROM "Expedientes"
                WHERE "Observaciones" IS NOT NULL
                AND BTRIM("Observaciones") <> '';
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Observaciones");
        }
    }
}
