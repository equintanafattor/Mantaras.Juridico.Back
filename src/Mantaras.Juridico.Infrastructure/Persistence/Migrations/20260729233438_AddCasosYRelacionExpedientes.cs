using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mantaras.Juridico.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCasosYRelacionExpedientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ExpedientesClientes");

            migrationBuilder.DropIndex(name: "IX_Expedientes_Caratula", table: "Expedientes");

            migrationBuilder.DropIndex(
                name: "IX_Expedientes_FaseInterna_Activo",
                table: "Expedientes"
            );

            migrationBuilder.DropColumn(name: "Derivado", table: "Expedientes");

            migrationBuilder.DropColumn(name: "FaseInterna", table: "Expedientes");

            migrationBuilder.DropColumn(name: "Observaciones", table: "Expedientes");

            migrationBuilder.DropColumn(name: "TipoTramite", table: "Expedientes");

            migrationBuilder.AlterColumn<string>(
                name: "Juzgado",
                table: "Expedientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "EstadoLegal",
                table: "Expedientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Caratula",
                table: "Expedientes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500
            );

            migrationBuilder.AddColumn<long>(
                name: "CasoId",
                table: "Expedientes",
                type: "bigint",
                nullable: true
            );

            migrationBuilder.AddColumn<long>(
                name: "ExpedientePadreId",
                table: "Expedientes",
                type: "bigint",
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "Casos",
                columns: table => new
                {
                    CasoId = table
                        .Column<long>(type: "bigint", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    Titulo = table.Column<string>(
                        type: "character varying(300)",
                        maxLength: 300,
                        nullable: false
                    ),
                    FaseInterna = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    TipoTramite = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: true
                    ),
                    Observaciones = table.Column<string>(
                        type: "character varying(2000)",
                        maxLength: 2000,
                        nullable: true
                    ),
                    FechaCreacion = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    UsuarioCreacion = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    FechaModificacion = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    UsuarioModificacion = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    Activo = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Casos", x => x.CasoId);
                }
            );

            migrationBuilder.CreateTable(
                name: "CasosClientes",
                columns: table => new
                {
                    CasoId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    TipoParticipacion = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    EsPrincipal = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CasosClientes", x => new { x.CasoId, x.ClienteId });
                    table.ForeignKey(
                        name: "FK_CasosClientes_Casos_CasoId",
                        column: x => x.CasoId,
                        principalTable: "Casos",
                        principalColumn: "CasoId",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_CasosClientes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.AlterColumn<long>(
                name: "CasoId",
                table: "Expedientes",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_CasoId",
                table: "Expedientes",
                column: "CasoId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_ExpedientePadreId",
                table: "Expedientes",
                column: "ExpedientePadreId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Casos_FaseInterna",
                table: "Casos",
                column: "FaseInterna"
            );

            migrationBuilder.CreateIndex(name: "IX_Casos_Titulo", table: "Casos", column: "Titulo");

            migrationBuilder.CreateIndex(
                name: "IX_CasosClientes_ClienteId",
                table: "CasosClientes",
                column: "ClienteId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Expedientes_Casos_CasoId",
                table: "Expedientes",
                column: "CasoId",
                principalTable: "Casos",
                principalColumn: "CasoId",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Expedientes_Expedientes_ExpedientePadreId",
                table: "Expedientes",
                column: "ExpedientePadreId",
                principalTable: "Expedientes",
                principalColumn: "ExpedienteId",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expedientes_Casos_CasoId",
                table: "Expedientes"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Expedientes_Expedientes_ExpedientePadreId",
                table: "Expedientes"
            );

            migrationBuilder.DropTable(name: "CasosClientes");

            migrationBuilder.DropTable(name: "Casos");

            migrationBuilder.DropIndex(name: "IX_Expedientes_CasoId", table: "Expedientes");

            migrationBuilder.DropIndex(
                name: "IX_Expedientes_ExpedientePadreId",
                table: "Expedientes"
            );

            migrationBuilder.DropColumn(name: "CasoId", table: "Expedientes");

            migrationBuilder.DropColumn(name: "ExpedientePadreId", table: "Expedientes");

            migrationBuilder.AlterColumn<string>(
                name: "Juzgado",
                table: "Expedientes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "EstadoLegal",
                table: "Expedientes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "Caratula",
                table: "Expedientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000
            );

            migrationBuilder.AddColumn<bool>(
                name: "Derivado",
                table: "Expedientes",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );

            migrationBuilder.AddColumn<string>(
                name: "FaseInterna",
                table: "Expedientes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Expedientes",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "TipoTramite",
                table: "Expedientes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true
            );

            migrationBuilder.CreateTable(
                name: "ExpedientesClientes",
                columns: table => new
                {
                    ExpedienteId = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<long>(type: "bigint", nullable: false),
                    EsPrincipal = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    TipoParticipacion = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ExpedientesClientes",
                        x => new { x.ExpedienteId, x.ClienteId }
                    );
                    table.ForeignKey(
                        name: "FK_ExpedientesClientes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_ExpedientesClientes_Expedientes_ExpedienteId",
                        column: x => x.ExpedienteId,
                        principalTable: "Expedientes",
                        principalColumn: "ExpedienteId",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_Caratula",
                table: "Expedientes",
                column: "Caratula"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_FaseInterna_Activo",
                table: "Expedientes",
                columns: new[] { "FaseInterna", "Activo" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesClientes_ClienteId",
                table: "ExpedientesClientes",
                column: "ClienteId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesClientes_ExpedienteId_EsPrincipal",
                table: "ExpedientesClientes",
                columns: new[] { "ExpedienteId", "EsPrincipal" }
            );
        }
    }
}
