using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModeloAgenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiasInhabiles",
                columns: table => new
                {
                    DiaInhabilId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiasInhabiles", x => x.DiaInhabilId);
                });

            migrationBuilder.CreateTable(
                name: "RecurrenciasAgenda",
                columns: table => new
                {
                    RecurrenciaAgendaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Frecuencia = table.Column<int>(type: "integer", nullable: false),
                    Intervalo = table.Column<int>(type: "integer", nullable: false),
                    DiasSemana = table.Column<int>(type: "integer", nullable: true),
                    DiaMes = table.Column<int>(type: "integer", nullable: true),
                    MesAnio = table.Column<int>(type: "integer", nullable: true),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    MaximoOcurrencias = table.Column<int>(type: "integer", nullable: true),
                    OcurrenciasGeneradas = table.Column<int>(type: "integer", nullable: false),
                    ProximaFecha = table.Column<DateOnly>(type: "date", nullable: true),
                    ZonaHoraria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "America/Argentina/Buenos_Aires"),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurrenciasAgenda", x => x.RecurrenciaAgendaId);
                    table.CheckConstraint("CK_RecurrenciasAgenda_DiaMes", "\"DiaMes\" IS NULL OR (\"DiaMes\" BETWEEN 1 AND 31)");
                    table.CheckConstraint("CK_RecurrenciasAgenda_DiasSemana", "\"DiasSemana\" IS NULL OR (\"DiasSemana\" BETWEEN 1 AND 127)");
                    table.CheckConstraint("CK_RecurrenciasAgenda_Fechas", "\"FechaFin\" IS NULL OR \"FechaFin\" >= \"FechaInicio\"");
                    table.CheckConstraint("CK_RecurrenciasAgenda_Intervalo", "\"Intervalo\" > 0");
                    table.CheckConstraint("CK_RecurrenciasAgenda_MaximoOcurrencias", "\"MaximoOcurrencias\" IS NULL OR \"MaximoOcurrencias\" > 0");
                    table.CheckConstraint("CK_RecurrenciasAgenda_MesAnio", "\"MesAnio\" IS NULL OR (\"MesAnio\" BETWEEN 1 AND 12)");
                });

            migrationBuilder.CreateTable(
                name: "TiposEntradaAgenda",
                columns: table => new
                {
                    TipoEntradaAgendaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEntradaAgenda", x => x.TipoEntradaAgendaId);
                });

            migrationBuilder.CreateTable(
                name: "EntradasAgenda",
                columns: table => new
                {
                    EntradaAgendaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoEntradaAgendaId = table.Column<long>(type: "bigint", nullable: false),
                    RecurrenciaAgendaId = table.Column<long>(type: "bigint", nullable: true),
                    Titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Prioridad = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    HoraFin = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    HoraVencimiento = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ZonaHoraria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "America/Argentina/Buenos_Aires"),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradasAgenda", x => x.EntradaAgendaId);
                    table.CheckConstraint("CK_EntradasAgenda_Fechas", "\"FechaFin\" IS NULL OR \"FechaFin\" >= \"FechaInicio\"");
                    table.CheckConstraint("CK_EntradasAgenda_HoraFin", "\"HoraFin\" IS NULL OR \"FechaFin\" IS NOT NULL");
                    table.CheckConstraint("CK_EntradasAgenda_HoraVencimiento", "\"HoraVencimiento\" IS NULL OR \"FechaVencimiento\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_EntradasAgenda_RecurrenciasAgenda_RecurrenciaAgendaId",
                        column: x => x.RecurrenciaAgendaId,
                        principalTable: "RecurrenciasAgenda",
                        principalColumn: "RecurrenciaAgendaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasAgenda_TiposEntradaAgenda_TipoEntradaAgendaId",
                        column: x => x.TipoEntradaAgendaId,
                        principalTable: "TiposEntradaAgenda",
                        principalColumn: "TipoEntradaAgendaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecordatoriosPredeterminadosTiposAgenda",
                columns: table => new
                {
                    RecordatorioPredeterminadoTipoAgendaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoEntradaAgendaId = table.Column<long>(type: "bigint", nullable: false),
                    BaseCalculo = table.Column<int>(type: "integer", nullable: false),
                    MinutosAnticipacion = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordatoriosPredeterminadosTiposAgenda", x => x.RecordatorioPredeterminadoTipoAgendaId);
                    table.CheckConstraint("CK_RecordatoriosPredeterminadosTiposAgenda_Anticipacion", "\"MinutosAnticipacion\" >= 0");
                    table.ForeignKey(
                        name: "FK_RecordatoriosPredeterminadosTiposAgenda_TiposEntradaAgenda_~",
                        column: x => x.TipoEntradaAgendaId,
                        principalTable: "TiposEntradaAgenda",
                        principalColumn: "TipoEntradaAgendaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReglasVencimiento",
                columns: table => new
                {
                    ReglaVencimientoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoEntradaAgendaId = table.Column<long>(type: "bigint", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CantidadDias = table.Column<int>(type: "integer", nullable: false),
                    TipoComputo = table.Column<int>(type: "integer", nullable: false),
                    SentidoCalculo = table.Column<int>(type: "integer", nullable: false),
                    PrioridadGenerada = table.Column<int>(type: "integer", nullable: false),
                    HoraSugerida = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasVencimiento", x => x.ReglaVencimientoId);
                    table.CheckConstraint("CK_ReglasVencimiento_CantidadDias", "\"CantidadDias\" >= 0");
                    table.ForeignKey(
                        name: "FK_ReglasVencimiento_TiposEntradaAgenda_TipoEntradaAgendaId",
                        column: x => x.TipoEntradaAgendaId,
                        principalTable: "TiposEntradaAgenda",
                        principalColumn: "TipoEntradaAgendaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntradasAgendaCasos",
                columns: table => new
                {
                    EntradaAgendaId = table.Column<long>(type: "bigint", nullable: false),
                    CasoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradasAgendaCasos", x => new { x.EntradaAgendaId, x.CasoId });
                    table.ForeignKey(
                        name: "FK_EntradasAgendaCasos_Casos_CasoId",
                        column: x => x.CasoId,
                        principalTable: "Casos",
                        principalColumn: "CasoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasAgendaCasos_EntradasAgenda_EntradaAgendaId",
                        column: x => x.EntradaAgendaId,
                        principalTable: "EntradasAgenda",
                        principalColumn: "EntradaAgendaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntradasAgendaClientes",
                columns: table => new
                {
                    EntradaAgendaId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradasAgendaClientes", x => new { x.EntradaAgendaId, x.ClienteId });
                    table.ForeignKey(
                        name: "FK_EntradasAgendaClientes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasAgendaClientes_EntradasAgenda_EntradaAgendaId",
                        column: x => x.EntradaAgendaId,
                        principalTable: "EntradasAgenda",
                        principalColumn: "EntradaAgendaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntradasAgendaExpedientes",
                columns: table => new
                {
                    EntradaAgendaId = table.Column<long>(type: "bigint", nullable: false),
                    ExpedienteId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradasAgendaExpedientes", x => new { x.EntradaAgendaId, x.ExpedienteId });
                    table.ForeignKey(
                        name: "FK_EntradasAgendaExpedientes_EntradasAgenda_EntradaAgendaId",
                        column: x => x.EntradaAgendaId,
                        principalTable: "EntradasAgenda",
                        principalColumn: "EntradaAgendaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntradasAgendaExpedientes_Expedientes_ExpedienteId",
                        column: x => x.ExpedienteId,
                        principalTable: "Expedientes",
                        principalColumn: "ExpedienteId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntradasAgendaResponsables",
                columns: table => new
                {
                    EntradaAgendaId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradasAgendaResponsables", x => new { x.EntradaAgendaId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_EntradasAgendaResponsables_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasAgendaResponsables_EntradasAgenda_EntradaAgendaId",
                        column: x => x.EntradaAgendaId,
                        principalTable: "EntradasAgenda",
                        principalColumn: "EntradaAgendaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecordatoriosAgenda",
                columns: table => new
                {
                    RecordatorioAgendaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntradaAgendaId = table.Column<long>(type: "bigint", nullable: false),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    FechaProgramadaUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Atendido = table.Column<bool>(type: "boolean", nullable: false),
                    FechaAtendidoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioAtendioId = table.Column<long>(type: "bigint", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordatoriosAgenda", x => x.RecordatorioAgendaId);
                    table.ForeignKey(
                        name: "FK_RecordatoriosAgenda_AspNetUsers_UsuarioAtendioId",
                        column: x => x.UsuarioAtendioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecordatoriosAgenda_EntradasAgenda_EntradaAgendaId",
                        column: x => x.EntradaAgendaId,
                        principalTable: "EntradasAgenda",
                        principalColumn: "EntradaAgendaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgendaGeneracionesReglas",
                columns: table => new
                {
                    AgendaGeneracionReglaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReglaVencimientoId = table.Column<long>(type: "bigint", nullable: false),
                    EntradaAgendaId = table.Column<long>(type: "bigint", nullable: false),
                    FechaBase = table.Column<DateOnly>(type: "date", nullable: false),
                    OrigenFechaBase = table.Column<int>(type: "integer", nullable: false),
                    EntradaAgendaOrigenId = table.Column<long>(type: "bigint", nullable: true),
                    CasoOrigenId = table.Column<long>(type: "bigint", nullable: true),
                    ExpedienteOrigenId = table.Column<long>(type: "bigint", nullable: true),
                    ObservacionOrigenId = table.Column<long>(type: "bigint", nullable: true),
                    UsuarioGeneracionId = table.Column<long>(type: "bigint", nullable: true),
                    ClaveIdempotencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaGeneracionesReglas", x => x.AgendaGeneracionReglaId);
                    table.CheckConstraint("CK_AgendaGeneracionesReglas_Origen", "(\"OrigenFechaBase\" = 1 AND num_nonnulls(\"EntradaAgendaOrigenId\", \"CasoOrigenId\", \"ExpedienteOrigenId\", \"ObservacionOrigenId\") = 0) OR (\"OrigenFechaBase\" = 2 AND \"EntradaAgendaOrigenId\" IS NOT NULL AND num_nonnulls(\"CasoOrigenId\", \"ExpedienteOrigenId\", \"ObservacionOrigenId\") = 0) OR (\"OrigenFechaBase\" = 3 AND \"CasoOrigenId\" IS NOT NULL AND num_nonnulls(\"EntradaAgendaOrigenId\", \"ExpedienteOrigenId\", \"ObservacionOrigenId\") = 0) OR (\"OrigenFechaBase\" = 4 AND \"ExpedienteOrigenId\" IS NOT NULL AND num_nonnulls(\"EntradaAgendaOrigenId\", \"CasoOrigenId\", \"ObservacionOrigenId\") = 0) OR (\"OrigenFechaBase\" = 5 AND \"ObservacionOrigenId\" IS NOT NULL AND num_nonnulls(\"EntradaAgendaOrigenId\", \"CasoOrigenId\", \"ExpedienteOrigenId\") = 0)");
                    table.ForeignKey(
                        name: "FK_AgendaGeneracionesReglas_AspNetUsers_UsuarioGeneracionId",
                        column: x => x.UsuarioGeneracionId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendaGeneracionesReglas_Casos_CasoOrigenId",
                        column: x => x.CasoOrigenId,
                        principalTable: "Casos",
                        principalColumn: "CasoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendaGeneracionesReglas_EntradasAgenda_EntradaAgendaId",
                        column: x => x.EntradaAgendaId,
                        principalTable: "EntradasAgenda",
                        principalColumn: "EntradaAgendaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgendaGeneracionesReglas_EntradasAgenda_EntradaAgendaOrigen~",
                        column: x => x.EntradaAgendaOrigenId,
                        principalTable: "EntradasAgenda",
                        principalColumn: "EntradaAgendaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendaGeneracionesReglas_Expedientes_ExpedienteOrigenId",
                        column: x => x.ExpedienteOrigenId,
                        principalTable: "Expedientes",
                        principalColumn: "ExpedienteId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendaGeneracionesReglas_Observaciones_ObservacionOrigenId",
                        column: x => x.ObservacionOrigenId,
                        principalTable: "Observaciones",
                        principalColumn: "ObservacionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendaGeneracionesReglas_ReglasVencimiento_ReglaVencimiento~",
                        column: x => x.ReglaVencimientoId,
                        principalTable: "ReglasVencimiento",
                        principalColumn: "ReglaVencimientoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecordatoriosPredeterminadosReglasVencimiento",
                columns: table => new
                {
                    RecordatorioPredeterminadoReglaVencimientoId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReglaVencimientoId = table.Column<long>(type: "bigint", nullable: false),
                    BaseCalculo = table.Column<int>(type: "integer", nullable: false),
                    MinutosAnticipacion = table.Column<int>(type: "integer", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioCreacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioModificacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordatoriosPredeterminadosReglasVencimiento", x => x.RecordatorioPredeterminadoReglaVencimientoId);
                    table.CheckConstraint("CK_RecordatoriosPredeterminadosReglasVencimiento_Anticipacion", "\"MinutosAnticipacion\" >= 0");
                    table.ForeignKey(
                        name: "FK_RecordatoriosPredeterminadosReglasVencimiento_ReglasVencimi~",
                        column: x => x.ReglaVencimientoId,
                        principalTable: "ReglasVencimiento",
                        principalColumn: "ReglaVencimientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TiposEntradaAgenda",
                columns: new[] { "TipoEntradaAgendaId", "Activo", "Color", "Descripcion", "FechaCreacion", "FechaModificacion", "Nombre", "UsuarioCreacion", "UsuarioModificacion" },
                values: new object[,]
                {
                    { 1L, true, "blue", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Tarea interna", "Sistema", null },
                    { 2L, true, "red", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Vencimiento procesal", "Sistema", null },
                    { 3L, true, "purple", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Audiencia o turno", "Sistema", null },
                    { 4L, true, "yellow", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Recordatorio general", "Sistema", null },
                    { 5L, true, "green", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Reunión o llamado", "Sistema", null }
                });

            migrationBuilder.Sql(
                """
                SELECT setval(
                    pg_get_serial_sequence('"TiposEntradaAgenda"', 'TipoEntradaAgendaId'),
                    (SELECT MAX("TipoEntradaAgendaId") FROM "TiposEntradaAgenda"),
                    true
                );
                """
            );

            migrationBuilder.CreateIndex(
                name: "IX_AgendaGeneracionesReglas_CasoOrigenId",
                table: "AgendaGeneracionesReglas",
                column: "CasoOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaGeneracionesReglas_ClaveIdempotencia",
                table: "AgendaGeneracionesReglas",
                column: "ClaveIdempotencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgendaGeneracionesReglas_EntradaAgendaId",
                table: "AgendaGeneracionesReglas",
                column: "EntradaAgendaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgendaGeneracionesReglas_EntradaAgendaOrigenId",
                table: "AgendaGeneracionesReglas",
                column: "EntradaAgendaOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaGeneracionesReglas_ExpedienteOrigenId",
                table: "AgendaGeneracionesReglas",
                column: "ExpedienteOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaGeneracionesReglas_ObservacionOrigenId",
                table: "AgendaGeneracionesReglas",
                column: "ObservacionOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaGeneracionesReglas_ReglaVencimientoId",
                table: "AgendaGeneracionesReglas",
                column: "ReglaVencimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaGeneracionesReglas_UsuarioGeneracionId",
                table: "AgendaGeneracionesReglas",
                column: "UsuarioGeneracionId");

            migrationBuilder.CreateIndex(
                name: "IX_DiasInhabiles_Activo",
                table: "DiasInhabiles",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_DiasInhabiles_Fecha",
                table: "DiasInhabiles",
                column: "Fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntradasAgenda_Activo",
                table: "EntradasAgenda",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasAgenda_FechaInicio_Estado",
                table: "EntradasAgenda",
                columns: new[] { "FechaInicio", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_EntradasAgenda_FechaVencimiento_Estado",
                table: "EntradasAgenda",
                columns: new[] { "FechaVencimiento", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_EntradasAgenda_RecurrenciaAgendaId",
                table: "EntradasAgenda",
                column: "RecurrenciaAgendaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasAgenda_TipoEntradaAgendaId",
                table: "EntradasAgenda",
                column: "TipoEntradaAgendaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasAgendaCasos_CasoId",
                table: "EntradasAgendaCasos",
                column: "CasoId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasAgendaClientes_ClienteId",
                table: "EntradasAgendaClientes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasAgendaExpedientes_ExpedienteId",
                table: "EntradasAgendaExpedientes",
                column: "ExpedienteId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasAgendaResponsables_UsuarioId",
                table: "EntradasAgendaResponsables",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordatoriosAgenda_EntradaAgendaId",
                table: "RecordatoriosAgenda",
                column: "EntradaAgendaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordatoriosAgenda_FechaProgramadaUtc_Atendido_Activo",
                table: "RecordatoriosAgenda",
                columns: new[] { "FechaProgramadaUtc", "Atendido", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_RecordatoriosAgenda_UsuarioAtendioId",
                table: "RecordatoriosAgenda",
                column: "UsuarioAtendioId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordatoriosPredeterminadosReglasVencimiento_ReglaVencimie~",
                table: "RecordatoriosPredeterminadosReglasVencimiento",
                columns: new[] { "ReglaVencimientoId", "BaseCalculo", "MinutosAnticipacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecordatoriosPredeterminadosTiposAgenda_TipoEntradaAgendaId~",
                table: "RecordatoriosPredeterminadosTiposAgenda",
                columns: new[] { "TipoEntradaAgendaId", "BaseCalculo", "MinutosAnticipacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurrenciasAgenda_ProximaFecha_Activo",
                table: "RecurrenciasAgenda",
                columns: new[] { "ProximaFecha", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_ReglasVencimiento_Nombre",
                table: "ReglasVencimiento",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReglasVencimiento_TipoEntradaAgendaId_Activo",
                table: "ReglasVencimiento",
                columns: new[] { "TipoEntradaAgendaId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_TiposEntradaAgenda_Activo",
                table: "TiposEntradaAgenda",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_TiposEntradaAgenda_Nombre",
                table: "TiposEntradaAgenda",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgendaGeneracionesReglas");

            migrationBuilder.DropTable(
                name: "DiasInhabiles");

            migrationBuilder.DropTable(
                name: "EntradasAgendaCasos");

            migrationBuilder.DropTable(
                name: "EntradasAgendaClientes");

            migrationBuilder.DropTable(
                name: "EntradasAgendaExpedientes");

            migrationBuilder.DropTable(
                name: "EntradasAgendaResponsables");

            migrationBuilder.DropTable(
                name: "RecordatoriosAgenda");

            migrationBuilder.DropTable(
                name: "RecordatoriosPredeterminadosReglasVencimiento");

            migrationBuilder.DropTable(
                name: "RecordatoriosPredeterminadosTiposAgenda");

            migrationBuilder.DropTable(
                name: "EntradasAgenda");

            migrationBuilder.DropTable(
                name: "ReglasVencimiento");

            migrationBuilder.DropTable(
                name: "RecurrenciasAgenda");

            migrationBuilder.DropTable(
                name: "TiposEntradaAgenda");
        }
    }
}
