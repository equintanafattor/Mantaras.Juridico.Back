using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosExpedienteAdministrativo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NumeroExpedienteAnses",
                table: "Casos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TipoBeneficioId",
                table: "Casos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TipoExpedienteAdministrativoId",
                table: "Casos",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TiposBeneficio",
                columns: table => new
                {
                    TipoBeneficioId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposBeneficio", x => x.TipoBeneficioId);
                });

            migrationBuilder.CreateTable(
                name: "TiposExpedienteAdministrativo",
                columns: table => new
                {
                    TipoExpedienteAdministrativoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposExpedienteAdministrativo", x => x.TipoExpedienteAdministrativoId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Casos_NumeroExpedienteAnses",
                table: "Casos",
                column: "NumeroExpedienteAnses");

            migrationBuilder.CreateIndex(
                name: "IX_Casos_TipoBeneficioId",
                table: "Casos",
                column: "TipoBeneficioId");

            migrationBuilder.CreateIndex(
                name: "IX_Casos_TipoExpedienteAdministrativoId",
                table: "Casos",
                column: "TipoExpedienteAdministrativoId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposBeneficio_Nombre",
                table: "TiposBeneficio",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposExpedienteAdministrativo_Nombre",
                table: "TiposExpedienteAdministrativo",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Casos_TiposBeneficio_TipoBeneficioId",
                table: "Casos",
                column: "TipoBeneficioId",
                principalTable: "TiposBeneficio",
                principalColumn: "TipoBeneficioId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Casos_TiposExpedienteAdministrativo_TipoExpedienteAdministr~",
                table: "Casos",
                column: "TipoExpedienteAdministrativoId",
                principalTable: "TiposExpedienteAdministrativo",
                principalColumn: "TipoExpedienteAdministrativoId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                """
                INSERT INTO "TiposBeneficio"
                    ("Nombre", "FechaCreacion", "UsuarioCreacion", "Activo")
                VALUES
                    ('JUBILACIÓN', CURRENT_TIMESTAMP, 'Sistema', TRUE),
                    ('PENSIÓN', CURRENT_TIMESTAMP, 'Sistema', TRUE);
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Casos_TiposBeneficio_TipoBeneficioId",
                table: "Casos");

            migrationBuilder.DropForeignKey(
                name: "FK_Casos_TiposExpedienteAdministrativo_TipoExpedienteAdministr~",
                table: "Casos");

            migrationBuilder.DropTable(
                name: "TiposBeneficio");

            migrationBuilder.DropTable(
                name: "TiposExpedienteAdministrativo");

            migrationBuilder.DropIndex(
                name: "IX_Casos_NumeroExpedienteAnses",
                table: "Casos");

            migrationBuilder.DropIndex(
                name: "IX_Casos_TipoBeneficioId",
                table: "Casos");

            migrationBuilder.DropIndex(
                name: "IX_Casos_TipoExpedienteAdministrativoId",
                table: "Casos");

            migrationBuilder.DropColumn(
                name: "NumeroExpedienteAnses",
                table: "Casos");

            migrationBuilder.DropColumn(
                name: "TipoBeneficioId",
                table: "Casos");

            migrationBuilder.DropColumn(
                name: "TipoExpedienteAdministrativoId",
                table: "Casos");
        }
    }
}
