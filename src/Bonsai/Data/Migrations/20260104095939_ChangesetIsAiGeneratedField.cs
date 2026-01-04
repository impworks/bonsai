using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonsai.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangesetIsAiGeneratedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAIGenerated",
                table: "Changes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAIGenerated",
                table: "Changes");
        }
    }
}
