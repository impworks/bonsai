using Microsoft.EntityFrameworkCore.Migrations;

namespace Bonsai.Data.Migrations
{
    public partial class ChangesGrouping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW ""ChangesGrouped""
                AS
                    SELECT
                        cc.""GroupKey"",
                        string_agg(CAST(cc.""Id"" AS VARCHAR), ',' ORDER BY cc.""Date"" DESC) AS ""Ids"",
                        max(cc.""Date"") AS ""Date""
                    FROM
                    (
                        SELECT 
	                        cg.*,
	                        CONCAT(
		                        cg.""AuthorId"",
		                        '__',
		                        cg.""Type"",
		                        '__',
		                        CASE
			                        WHEN cg.""RevertedChangesetId"" IS NOT NULL THEN 'Restored'
			                        WHEN cg.""OriginalState"" IS NULL THEN 'Created'
			                        WHEN cg.""UpdatedState"" IS NULL THEN 'Removed'
			                        ELSE 'Updated'
		                        END,
		                        CASE
			                        WHEN cg.""EditedMediaId"" IS NOT NULL THEN
				                        NULL
			                        ELSE
				                        CONCAT('__', COALESCE(cg.""EditedPageId"", cg.""EditedRelationId"") || '')
		                        END,
		                        '__',
		                        (EXTRACT(EPOCH FROM cg.""Date"") / 3600)::int
	                        ) AS ""GroupKey""
                        FROM PUBLIC.""Changes"" AS cg
                    ) AS cc
                    GROUP BY cc.""GroupKey""
                    ORDER BY ""Date"" DESC
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW ""ChangesGrouped""");
        }
    }
}
