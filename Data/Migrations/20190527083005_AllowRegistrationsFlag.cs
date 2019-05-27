using Microsoft.EntityFrameworkCore.Migrations;

namespace Bonsai.Data.Migrations
{
    public partial class AllowRegistrationsFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowRegistration",
                table: "Config",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowRegistration",
                table: "Config");
        }
    }
}
