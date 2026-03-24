using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MiProyecto.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAvisoAndDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                table: "OutboxMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_OutboxMessage_OutboxState_OutboxId",
                table: "OutboxMessage");

            migrationBuilder.CreateTable(
                name: "Avisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroPoliza = table.Column<int>(type: "integer", nullable: false),
                    NumeroAviso = table.Column<int>(type: "integer", nullable: false),
                    NombreCliente = table.Column<string>(type: "text", nullable: true),
                    RutCliente = table.Column<string>(type: "text", nullable: true),
                    Direccion = table.Column<string>(type: "text", nullable: true),
                    FechaCancelacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetalleAvisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroCuota = table.Column<int>(type: "integer", nullable: false),
                    TotalCuota = table.Column<decimal>(type: "numeric", nullable: false),
                    Observacion = table.Column<string>(type: "text", nullable: true),
                    AvisoId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleAvisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleAvisos_Avisos_AvisoId",
                        column: x => x.AvisoId,
                        principalTable: "Avisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_NumeroPoliza_NumeroAviso",
                table: "Avisos",
                columns: new[] { "NumeroPoliza", "NumeroAviso" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleAvisos_AvisoId",
                table: "DetalleAvisos",
                column: "AvisoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleAvisos");

            migrationBuilder.DropTable(
                name: "Avisos");

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId" },
                principalTable: "InboxState",
                principalColumns: new[] { "MessageId", "ConsumerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_OutboxMessage_OutboxState_OutboxId",
                table: "OutboxMessage",
                column: "OutboxId",
                principalTable: "OutboxState",
                principalColumn: "OutboxId");
        }
    }
}
