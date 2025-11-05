using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalLogParser.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadedLogFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedLogFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LogFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileContent = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedLogFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadedLogFiles_LogFiles_LogFileId",
                        column: x => x.LogFileId,
                        principalTable: "LogFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedLogFiles_LogFileId",
                table: "UploadedLogFiles",
                column: "LogFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedLogFiles");
        }
    }
}
