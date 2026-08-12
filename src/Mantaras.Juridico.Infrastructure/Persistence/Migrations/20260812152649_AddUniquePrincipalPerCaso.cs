using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePrincipalPerCaso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_CasoId_Principal",
                table: "Expedientes",
                column: "CasoId",
                unique: true,
                filter: "\"TipoExpediente\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Expedientes_CasoId_Principal",
                table: "Expedientes");
        }
    }
}
