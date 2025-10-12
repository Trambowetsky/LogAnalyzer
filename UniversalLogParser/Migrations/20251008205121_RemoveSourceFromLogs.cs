using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniversalLogParser.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSourceFromLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "LogEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "LogEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
