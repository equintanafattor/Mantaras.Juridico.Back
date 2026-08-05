using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpedientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaveSeguridadSocial",
                table: "Clientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Expedientes",
                columns: table => new
                {
                    ExpedienteId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroExpediente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Caratula = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Juzgado = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FaseInterna = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EstadoLegal = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    TipoTramite = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Derivado = table.Column<bool>(type: "boolean", nullable: false),
                    Observaciones = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expedientes", x => x.ExpedienteId);
                });

            migrationBuilder.CreateTable(
                name: "ExpedientesClientes",
                columns: table => new
                {
                    ExpedienteId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    TipoParticipacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EsPrincipal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpedientesClientes", x => new { x.ExpedienteId, x.ClienteId });
                    table.ForeignKey(
                        name: "FK_ExpedientesClientes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExpedientesClientes_Expedientes_ExpedienteId",
                        column: x => x.ExpedienteId,
                        principalTable: "Expedientes",
                        principalColumn: "ExpedienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_Caratula",
                table: "Expedientes",
                column: "Caratula");

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_FaseInterna_Activo",
                table: "Expedientes",
                columns: new[] { "FaseInterna", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_NumeroExpediente",
                table: "Expedientes",
                column: "NumeroExpediente");

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesClientes_ClienteId",
                table: "ExpedientesClientes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesClientes_ExpedienteId_EsPrincipal",
                table: "ExpedientesClientes",
                columns: new[] { "ExpedienteId", "EsPrincipal" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpedientesClientes");

            migrationBuilder.DropTable(
                name: "Expedientes");

            migrationBuilder.DropColumn(
                name: "ClaveSeguridadSocial",
                table: "Clientes");
        }
    }
}
