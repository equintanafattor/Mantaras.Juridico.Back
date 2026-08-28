using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRelacionesFamiliares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RelacionesFamiliares",
                columns: table => new
                {
                    RelacionFamiliarId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteAId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteBId = table.Column<long>(type: "bigint", nullable: false),
                    ParentescoDeB = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelacionesFamiliares", x => x.RelacionFamiliarId);
                    table.CheckConstraint("CK_RelacionesFamiliares_ClientesOrdenados", "\"ClienteAId\" < \"ClienteBId\"");
                    table.CheckConstraint("CK_RelacionesFamiliares_ParentescoValido", "\"ParentescoDeB\" BETWEEN 1 AND 10");
                    table.ForeignKey(
                        name: "FK_RelacionesFamiliares_Clientes_ClienteAId",
                        column: x => x.ClienteAId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelacionesFamiliares_Clientes_ClienteBId",
                        column: x => x.ClienteBId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesFamiliares_ClienteAId_ClienteBId",
                table: "RelacionesFamiliares",
                columns: new[] { "ClienteAId", "ClienteBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesFamiliares_ClienteBId",
                table: "RelacionesFamiliares",
                column: "ClienteBId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelacionesFamiliares");
        }
    }
}
