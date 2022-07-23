using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonsai.Data.Migrations
{
    public partial class ChangesetCleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChangeType",
                table: "Changes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Changes",
                newName: "EntityType");

            migrationBuilder.DropColumn(
                name: "OriginalState",
                table: "Changes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangeType",
                table: "Changes");

            migrationBuilder.RenameColumn(
                name: "EntityType",
                table: "Changes",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "OriginalState",
                table: "Changes",
                type: "text",
                nullable: true);
        }
    }
}
