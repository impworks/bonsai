using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Bonsai.Data.Migrations
{
    public partial class DynamicConfigJson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DynamicConfig",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicConfig", x => x.Id);
                });

            migrationBuilder.Sql(@$"
                INSERT INTO ""DynamicConfig"" (""Id"", ""Value"")
                SELECT
                    ""Id"",
                    CONCAT(
	                    '{{""Title"": ""',
                        ""Title"",
                        '"", ""AllowGuests"": ',
                        CASE WHEN ""AllowGuests"" THEN 'true' ELSE 'false' END,
                        ', ""AllowRegistration"": ',
                        CASE WHEN ""AllowRegistration"" THEN 'true' ELSE 'false' END,
                        '}}'
                    ) AS ""Value""
                FROM ""Config""
            ");

            migrationBuilder.DropTable(
                name: "Config");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Config",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AllowGuests = table.Column<bool>(nullable: false),
                    AllowRegistration = table.Column<bool>(nullable: false),
                    Title = table.Column<string>(maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config", x => x.Id);
                });

            migrationBuilder.Sql(@"
                INSERT INTO ""Config""
                    (""Id"", ""AllowGuests"", ""AllowRegistration"", ""Title"")
                SELECT
                    ""Id"", 0, 1, 'Bonsai'
                FROM ""DynamicConfig""
            ");

            migrationBuilder.DropTable(
                name: "DynamicConfig");
        }
    }
}
