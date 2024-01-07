using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonsai.Data.Migrations
{
    /// <inheritdoc />
    public partial class TreeKinds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "TreeLayouts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PageId",
                table: "TreeLayouts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreeLayouts_PageId",
                table: "TreeLayouts",
                column: "PageId");

            migrationBuilder.AddForeignKey(
                name: "FK_TreeLayouts_Pages_PageId",
                table: "TreeLayouts",
                column: "PageId",
                principalTable: "Pages",
                principalColumn: "Id");

            if (migrationBuilder.IsNpgsql())
            {
                migrationBuilder.Sql(@"
UPDATE PUBLIC.""TreeLayouts"" SET ""Kind"" = 1;
UPDATE PUBLIC.""DynamicConfig"" SET ""Value"" = (""Value""::jsonb || '{""TreeKinds"": 1}'::jsonb)::text;
                ");
            }
            else
            {
                migrationBuilder.Sql(@"
UPDATE TreeLayouts Set Kind = 1;
UPDATE DynamicConfig SET Value = json_insert(Value, '$.TreeKinds', 1);
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TreeLayouts_Pages_PageId",
                table: "TreeLayouts");

            migrationBuilder.DropIndex(
                name: "IX_TreeLayouts_PageId",
                table: "TreeLayouts");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "TreeLayouts");

            migrationBuilder.DropColumn(
                name: "PageId",
                table: "TreeLayouts");
        }
    }
}
