using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalLogParser.Migrations
{
    /// <inheritdoc />
    public partial class AddLogFileSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "LogFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "LogFiles");
        }
    }
}
