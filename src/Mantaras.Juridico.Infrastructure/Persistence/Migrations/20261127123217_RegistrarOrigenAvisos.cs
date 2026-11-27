using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RegistrarOrigenAvisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Origen",
                table: "RecordatoriosAgenda",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ReglaVencimientoOrigenId",
                table: "RecordatoriosAgenda",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TipoEntradaAgendaOrigenId",
                table: "RecordatoriosAgenda",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordatoriosAgenda_ReglaVencimientoOrigenId",
                table: "RecordatoriosAgenda",
                column: "ReglaVencimientoOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordatoriosAgenda_TipoEntradaAgendaOrigenId",
                table: "RecordatoriosAgenda",
                column: "TipoEntradaAgendaOrigenId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RecordatoriosAgenda_Origen",
                table: "RecordatoriosAgenda",
                sql: "(\"Origen\" IN (0, 1) AND num_nonnulls(\"TipoEntradaAgendaOrigenId\", \"ReglaVencimientoOrigenId\") = 0) OR (\"Origen\" = 2 AND \"TipoEntradaAgendaOrigenId\" IS NOT NULL AND \"ReglaVencimientoOrigenId\" IS NULL) OR (\"Origen\" = 3 AND \"ReglaVencimientoOrigenId\" IS NOT NULL AND \"TipoEntradaAgendaOrigenId\" IS NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_RecordatoriosAgenda_ReglasVencimiento_ReglaVencimientoOrige~",
                table: "RecordatoriosAgenda",
                column: "ReglaVencimientoOrigenId",
                principalTable: "ReglasVencimiento",
                principalColumn: "ReglaVencimientoId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordatoriosAgenda_TiposEntradaAgenda_TipoEntradaAgendaOri~",
                table: "RecordatoriosAgenda",
                column: "TipoEntradaAgendaOrigenId",
                principalTable: "TiposEntradaAgenda",
                principalColumn: "TipoEntradaAgendaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecordatoriosAgenda_ReglasVencimiento_ReglaVencimientoOrige~",
                table: "RecordatoriosAgenda");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordatoriosAgenda_TiposEntradaAgenda_TipoEntradaAgendaOri~",
                table: "RecordatoriosAgenda");

            migrationBuilder.DropIndex(
                name: "IX_RecordatoriosAgenda_ReglaVencimientoOrigenId",
                table: "RecordatoriosAgenda");

            migrationBuilder.DropIndex(
                name: "IX_RecordatoriosAgenda_TipoEntradaAgendaOrigenId",
                table: "RecordatoriosAgenda");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RecordatoriosAgenda_Origen",
                table: "RecordatoriosAgenda");

            migrationBuilder.DropColumn(
                name: "Origen",
                table: "RecordatoriosAgenda");

            migrationBuilder.DropColumn(
                name: "ReglaVencimientoOrigenId",
                table: "RecordatoriosAgenda");

            migrationBuilder.DropColumn(
                name: "TipoEntradaAgendaOrigenId",
                table: "RecordatoriosAgenda");
        }
    }
}
