using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalLogParser.Migrations
{
    /// <inheritdoc />
    public partial class AddLogFileRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LogFileId",
                table: "LogEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LogFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogEntries_LogFileId",
                table: "LogEntries",
                column: "LogFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_LogEntries_LogFiles_LogFileId",
                table: "LogEntries",
                column: "LogFileId",
                principalTable: "LogFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogEntries_LogFiles_LogFileId",
                table: "LogEntries");

            migrationBuilder.DropTable(
                name: "LogFiles");

            migrationBuilder.DropIndex(
                name: "IX_LogEntries_LogFileId",
                table: "LogEntries");

            migrationBuilder.DropColumn(
                name: "LogFileId",
                table: "LogEntries");
        }
    }
}
