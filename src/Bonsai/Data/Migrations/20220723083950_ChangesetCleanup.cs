﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bonsai.Data.Migrations
{
    public partial class ChangesetCleanup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // sic! not required for SQLite because either the DB is empty or data is already migrated inside PostgreSQL
            if (migrationBuilder.IsNpgsql())
            {
                migrationBuilder.AddColumn<int>(
                    name: "ChangeType",
                    table: "Changes",
                    type: "integer",
                    nullable: false,
                    defaultValue: 0);

                migrationBuilder.RenameColumn(
                    name: "Type",
                    table: "Changes",
                    newName: "EntityType");

                migrationBuilder.Sql(@"DROP VIEW ""ChangesGrouped""");

                migrationBuilder.Sql(@"
                    WITH
                      c1 AS (
                        SELECT
                           c.""Id"",
                           c.""Date"",
                           COALESCE(c.""EditedPageId"", c.""EditedMediaId"", c.""EditedRelationId"") AS ""EntityId"",
                           c.""RevertedChangesetId"",
                           CASE WHEN c.""UpdatedState"" IS NULL THEN 1 ELSE 0 END AS ""IsNull""
                        FROM ""Changes"" AS c
                      ),
                      c2 AS (
                        SELECT
                          c1.*,
                          COALESCE(LEAD(c1.""IsNull"") OVER (PARTITION BY ""EntityId"" ORDER BY ""Date"" DESC), 1) AS ""PrevIsNull""
                        FROM c1
                      ),
                      c3 AS (
                        SELECT
                          c2.""Id"",
                          CASE
                            WHEN c2.""RevertedChangesetId"" IS NOT NULL THEN 3
                            WHEN c2.""PrevIsNull"" = 1 AND c2.""IsNull"" = 0 THEN 0
                            WHEN c2.""PrevIsNull"" = 0 AND c2.""IsNull"" = 1 THEN 2
                            ELSE 1
                          END AS ""ChangeType""
                        FROM c2
                      )
                    UPDATE ""Changes""
                    SET ""ChangeType"" = c3.""ChangeType""
                    FROM c3
                    WHERE ""Changes"".""Id"" = c3.""Id""
                ");

                migrationBuilder.DropColumn(
                    name: "OriginalState",
                    table: "Changes");

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
		        cg.""EntityType"",
		        '__',
		        CASE cg.""ChangeType""
			        WHEN 3 THEN 'Restored'
			        WHEN 0 THEN 'Created'
			        WHEN 2 THEN 'Removed'
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
            else
            {
                // todo: ordering in string_agg!

                migrationBuilder.Sql(@"
DROP VIEW ChangesGrouped;

ALTER TABLE Changes ADD ChangeType INTEGER NOT NULL DEFAULT 0;
ALTER TABLE Changes RENAME COLUMN Type TO EntityType;
ALTER TABLE Changes DROP COLUMN OriginalState;

CREATE VIEW ChangesGrouped
AS
    SELECT
        GroupKey,
        group_concat(Id, ',' ORDER BY Date DESC) AS Ids,
        max(Date) AS Date
    FROM
    (
        SELECT 
	        cg.*,
	        (
		        cg.AuthorId ||
		        '__' ||
		        cg.EntityType ||
		        '__' ||
		        CASE cg.ChangeType
                    WHEN 3 THEN 'Restored'
                    WHEN 0 THEN 'Created'
                    WHEN 2 THEN 'Removed'
                    ELSE 'Updated'
                END ||
		        CASE
			        WHEN cg.EditedMediaId IS NOT NULL THEN
				        ''
			        ELSE
				        ('__' || COALESCE(cg.EditedPageId, cg.EditedRelationId, ''))
		        END ||
		        '__' ||
		        CAST(unixepoch(cg.Date) / 3600 as int)
	        ) AS GroupKey
        FROM Changes AS cg
    ) AS cc
    GROUP BY GroupKey
    ORDER BY Date DESC
                ");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (!migrationBuilder.IsNpgsql())
                throw new Exception("Backwards migration on SQLite is not supported.");

            migrationBuilder.AddColumn<string>(
                name: "OriginalState",
                table: "Changes",
                type: "text",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "EntityType",
                table: "Changes",
                newName: "Type");

            migrationBuilder.Sql(@"
                WITH
                  c1 AS (
                    SELECT
                       c.""Id"",
                       c.""Date"",
                       COALESCE(c.""EditedPageId"", c.""EditedMediaId"", c.""EditedRelationId"") AS ""EntityId"",
                       c.""UpdatedState""
                    FROM ""Changes"" AS c
                  ),
                  c2 AS (
                    SELECT
                      c1.*,
                      LEAD(c1.""UpdatedState"") OVER (PARTITION BY ""EntityId"" ORDER BY ""Date"" DESC) AS ""OriginalState""
                    FROM c1
                  )

                UPDATE ""Changes""
                SET ""OriginalState"" = c2.""OriginalState""
                FROM c2
                WHERE ""Changes"".""Id"" = c2.""Id""
            ");

            migrationBuilder.Sql(@"DROP VIEW ""ChangesGrouped""");

            migrationBuilder.DropColumn(
                name: "ChangeType",
                table: "Changes");

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
    }
}
