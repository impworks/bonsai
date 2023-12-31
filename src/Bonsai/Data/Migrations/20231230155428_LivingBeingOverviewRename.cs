using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonsai.Data.Migrations
{
    /// <inheritdoc />
    public partial class LivingBeingOverviewRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "LivingBeingOverview", newName: "LivingBeingOverviews");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(name: "LivingBeingOverviews", newName: "LivingBeingOverview");
        }
    }
}
