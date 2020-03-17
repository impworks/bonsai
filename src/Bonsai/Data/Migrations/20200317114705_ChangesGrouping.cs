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
                        min(cc.""Date"") AS ""Date""
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
                                WHEN cg.""Type"" = 1 AND cg.""EditedMediaId"" IS NOT NULL THEN
                                    'UploadedMedia'
                                ELSE
                                    COALESCE(cg.""EditedPageId"", cg.""EditedMediaId"", cg.""EditedRelationId"") || ''
                            END,
                            '_',
                            (EXTRACT(EPOCH FROM cg.""Date"") / 3600)::int
                        ) AS ""GroupKey""
                        FROM PUBLIC.""Changes"" AS cg
                    ) AS cc
                    GROUP BY cc.""GroupKey""
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW ""ChangesGrouped""");
        }
    }
}
