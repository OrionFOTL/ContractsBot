using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MafiaContractsBot.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    ThreadId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.ThreadId);
                });

            migrationBuilder.CreateTable(
                name: "ContractUsers",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompletedContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ContractThreadId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Points = table.Column<int>(type: "INTEGER", nullable: false),
                    CompletedOn = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletedContracts_ContractUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ContractUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletedContracts_Contracts_ContractThreadId",
                        column: x => x.ContractThreadId,
                        principalTable: "Contracts",
                        principalColumn: "ThreadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedContracts_ContractThreadId",
                table: "CompletedContracts",
                column: "ContractThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedContracts_UserId",
                table: "CompletedContracts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletedContracts");

            migrationBuilder.DropTable(
                name: "ContractUsers");

            migrationBuilder.DropTable(
                name: "Contracts");
        }
    }
}
