using Microsoft.EntityFrameworkCore.Migrations;

namespace Bonsai.Data.Migrations
{
    public partial class ChangesGrouping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE VIEW PUBLIC.""ChangesGrouped"" AS
                SELECT
	                ""Id"",
	                CONCAT(
		                ""AuthorId"",
		                '__',
		                ""Type"",
		                '__',
		                CASE
			                WHEN ""Type"" = 1 AND ""EditedMediaId"" IS NOT NULL THEN
				                'UploadedMedia'
			                ELSE
				                COALESCE(""EditedPageId"", ""EditedMediaId"", ""EditedRelationId"") || ''
		                END,
		                '_',
		                (EXTRACT(EPOCH FROM ""Date"") / 3600)::int
	                ) AS ""GroupKey""
                FROM PUBLIC.""Changes""
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW ""ChangesGrouped""");
        }
    }
}
