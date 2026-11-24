using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarImportacionFeriadosNacionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaFuente",
                table: "DiasInhabiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaSincronizacion",
                table: "DiasInhabiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fuente",
                table: "DiasInhabiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ModificadoManualmente",
                table: "DiasInhabiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Origen",
                table: "DiasInhabiles",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int[]>(
                name: "AniosSinCalendario",
                table: "AgendaGeneracionesReglas",
                type: "integer[]",
                nullable: false,
                defaultValueSql: "'{}'::integer[]");

            migrationBuilder.CreateTable(
                name: "CalendariosFeriadosImportados",
                columns: table => new
                {
                    Anio = table.Column<int>(type: "integer", nullable: false),
                    Fuente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UrlFuente = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FechaPublicacionFuente = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaPrimeraSincronizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaSincronizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioSincronizacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UltimaClaveConfirmacion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Creados = table.Column<int>(type: "integer", nullable: false),
                    Actualizados = table.Column<int>(type: "integer", nullable: false),
                    Existentes = table.Column<int>(type: "integer", nullable: false),
                    Conflictos = table.Column<int>(type: "integer", nullable: false),
                    RevisionIncluidos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendariosFeriadosImportados", x => x.Anio);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiasInhabiles_FechaFuente",
                table: "DiasInhabiles",
                column: "FechaFuente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendariosFeriadosImportados");

            migrationBuilder.DropIndex(
                name: "IX_DiasInhabiles_FechaFuente",
                table: "DiasInhabiles");

            migrationBuilder.DropColumn(
                name: "FechaFuente",
                table: "DiasInhabiles");

            migrationBuilder.DropColumn(
                name: "FechaSincronizacion",
                table: "DiasInhabiles");

            migrationBuilder.DropColumn(
                name: "Fuente",
                table: "DiasInhabiles");

            migrationBuilder.DropColumn(
                name: "ModificadoManualmente",
                table: "DiasInhabiles");

            migrationBuilder.DropColumn(
                name: "Origen",
                table: "DiasInhabiles");

            migrationBuilder.DropColumn(
                name: "AniosSinCalendario",
                table: "AgendaGeneracionesReglas");
        }
    }
}
