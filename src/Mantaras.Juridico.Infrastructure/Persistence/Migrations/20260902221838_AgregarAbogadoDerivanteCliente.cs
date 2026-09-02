using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAbogadoDerivanteCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DerivadoPor",
                table: "Clientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DerivadoPorEmail",
                table: "Clientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DerivadoPorTelefono",
                table: "Clientes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DerivadoPor",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DerivadoPorEmail",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "DerivadoPorTelefono",
                table: "Clientes");
        }
    }
}
