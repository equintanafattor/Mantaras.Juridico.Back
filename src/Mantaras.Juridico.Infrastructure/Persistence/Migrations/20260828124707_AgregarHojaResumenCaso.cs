using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHojaResumenCaso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HojasResumenCasos",
                columns: table => new
                {
                    CasoId = table.Column<long>(type: "bigint", nullable: false),
                    TieneCalculoPrevio = table.Column<bool>(type: "boolean", nullable: true),
                    HaberInicialReajustadoCaracteristicas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HaberInicialPbu = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    HaberInicialObservacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    HaberInicialMonto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MovilidadActualizacionMes = table.Column<int>(type: "integer", nullable: true),
                    MovilidadActualizacionAnio = table.Column<int>(type: "integer", nullable: true),
                    MovilidadObservaciones = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MovilidadMonto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RetroactivoFechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    RetroactivoFechaActualizacion = table.Column<DateOnly>(type: "date", nullable: true),
                    RetroactivoObservacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RetroactivoMonto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HojasResumenCasos", x => x.CasoId);
                    table.CheckConstraint("CK_HojasResumenCasos_PeriodoMovilidad", "(\r\n    \"MovilidadActualizacionMes\" IS NULL\r\n    AND \"MovilidadActualizacionAnio\" IS NULL\r\n)\r\nOR\r\n(\r\n    \"MovilidadActualizacionMes\" IS NOT NULL\r\n    AND \"MovilidadActualizacionAnio\" IS NOT NULL\r\n    AND \"MovilidadActualizacionMes\" BETWEEN 1 AND 12\r\n    AND \"MovilidadActualizacionAnio\" BETWEEN 1 AND 9999\r\n)");
                    table.ForeignKey(
                        name: "FK_HojasResumenCasos_Casos_CasoId",
                        column: x => x.CasoId,
                        principalTable: "Casos",
                        principalColumn: "CasoId",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HojasResumenCasos");
        }
    }
}
