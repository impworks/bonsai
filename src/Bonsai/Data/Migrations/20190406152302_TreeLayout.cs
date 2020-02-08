using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bonsai.Data.Migrations
{
    public partial class TreeLayout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TreeLayoutId",
                table: "Pages",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TreeLayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GenerationDate = table.Column<DateTimeOffset>(nullable: false),
                    LayoutJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreeLayouts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pages_TreeLayoutId",
                table: "Pages",
                column: "TreeLayoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_TreeLayouts_TreeLayoutId",
                table: "Pages",
                column: "TreeLayoutId",
                principalTable: "TreeLayouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pages_TreeLayouts_TreeLayoutId",
                table: "Pages");

            migrationBuilder.DropTable(
                name: "TreeLayouts");

            migrationBuilder.DropIndex(
                name: "IX_Pages_TreeLayoutId",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "TreeLayoutId",
                table: "Pages");
        }
    }
}
