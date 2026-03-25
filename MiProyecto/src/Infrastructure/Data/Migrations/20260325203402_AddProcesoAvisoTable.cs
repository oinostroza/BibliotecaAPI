using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MiProyecto.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcesoAvisoTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcesosAvisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AvisoId = table.Column<int>(type: "integer", nullable: false),
                    TipoProceso = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    RutaArchivo = table.Column<string>(type: "text", nullable: true),
                    ErrorMensaje = table.Column<string>(type: "text", nullable: true),
                    FechaFinalizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcesosAvisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcesosAvisos_Avisos_AvisoId",
                        column: x => x.AvisoId,
                        principalTable: "Avisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcesosAvisos_AvisoId_TipoProceso",
                table: "ProcesosAvisos",
                columns: new[] { "AvisoId", "TipoProceso" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcesosAvisos");
        }
    }
}
