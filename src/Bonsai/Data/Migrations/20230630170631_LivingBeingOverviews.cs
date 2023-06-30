using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonsai.Data.Migrations
{
    /// <inheritdoc />
    public partial class LivingBeingOverviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LivingBeingOverview",
                columns: table => new
                {
                    PageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gender = table.Column<bool>(type: "boolean", nullable: true),
                    BirthDate = table.Column<string>(type: "text", nullable: true),
                    DeathDate = table.Column<string>(type: "text", nullable: true),
                    IsDead = table.Column<bool>(type: "boolean", nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: true),
                    MaidenName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivingBeingOverview", x => x.PageId);
                    table.ForeignKey(
                        name: "FK_LivingBeingOverview_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id");
                });

            migrationBuilder.Sql(@"
                    INSERT INTO ""LivingBeingOverview""
                        (""PageId"", ""BirthDate"", ""DeathDate"", ""IsDead"", ""Gender"", ""ShortName"", ""MaidenName"")
                    SELECT
                        t.""Id"" AS ""PageId"",
                        t.""BirthDate"",
                        t.""DeathDate"",
                        t.""IsDead"",
                        t.""Gender"",
                        COALESCE(
                            t.""Nickname"",
                            CASE
                                WHEN t.""LastName"" IS NULL THEN NULL
                                ELSE CONCAT(t.""FirstName"", ' ', t.""LastName"")
                            END
                        ) AS ""ShortName"",
                        CASE
                            WHEN t.""MaidenName"" = t.""LastName"" THEN NULL
                            ELSE t.""MaidenName""
                        END AS ""MaidenName""
                    FROM (
                        SELECT
                            p.""Id"",
                            p.""Facts""::json#>>'{Main.Name,Values,-1,FirstName}' AS ""FirstName"",
                            p.""Facts""::json#>>'{Main.Name,Values,-1,LastName}' AS ""LastName"",
                            p.""Facts""::json#>>'{Main.Name,Value}' AS ""Nickname"",
                            p.""Facts""::json#>>'{Main.Name,Values,0,LastName}' AS ""MaidenName"",
                            p.""Facts""::json#>>'{Birth.Date,Value}' AS ""BirthDate"",
                            p.""Facts""::json#>>'{Death.Date,Value}' AS ""DeathDate"",
                            p.""Facts""::json->'Death.Date' IS NOT NULL AS ""IsDead"",
                            CAST(p.""Facts""::json#>>'{Bio.Gender,IsMale}' AS BOOLEAN) AS ""Gender""
                        FROM ""Pages"" AS p
                        WHERE p.""IsDeleted"" = false AND p.""Type"" IN (0, 1)
                    ) AS t
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LivingBeingOverview");
        }
    }
}
