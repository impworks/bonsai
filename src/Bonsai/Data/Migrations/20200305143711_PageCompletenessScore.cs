using Microsoft.EntityFrameworkCore.Migrations;

namespace Bonsai.Data.Migrations
{
    public partial class PageCompletenessScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE VIEW ""PagesScored""
AS
SELECT
	pp.""Id"",
	pp.""Title"",
	pp.""Key"",
	pp.""Type"",
	pp.""MainPhotoId"",
	pp.""CreationDate"",
	pp.""LastUpdateDate"",
	pp.""IsDeleted"",
	pp.""HasText"",
	pp.""HasPhoto"",
	pp.""HasRelations"",
	pp.""HasHumanName"",
	pp.""HasBirthday"",
	pp.""HasBirthPlace"",
	pp.""HasGender"",
	pp.""HasAnimalName"",
	pp.""HasAnimalSpecies"",
	pp.""HasEventDate"",
	pp.""HasLocationAddress"",
	CASE
		WHEN pp.""Type"" = 0	THEN
			(CASE WHEN pp.""HasText"" THEN 30 ELSE 0 END)
		   + (CASE WHEN pp.""HasPhoto"" THEN 15 ELSE 0 END)
  		   + (CASE WHEN pp.""HasRelations"" THEN 15 ELSE 0 END)
  		   + (CASE WHEN pp.""HasHumanName"" THEN 10 ELSE 0 END)
  		   + (CASE WHEN pp.""HasBirthday"" THEN 10 ELSE 0 END)
  		   + (CASE WHEN pp.""HasBirthPlace"" THEN 10 ELSE 0 END)
  		   + (CASE WHEN pp.""HasGender"" THEN 10 ELSE 0 END)
  		WHEN pp.""Type"" = 1 THEN
  			(CASE WHEN pp.""HasText"" THEN 30 ELSE 0 END)
		   + (CASE WHEN pp.""HasPhoto"" THEN 20 ELSE 0 END)
  		   + (CASE WHEN pp.""HasRelations"" THEN 20 ELSE 0 END)
 		   + (CASE WHEN pp.""HasBirthday"" THEN 10 ELSE 0 END)
  		   + (CASE WHEN pp.""HasAnimalName"" THEN 10 ELSE 0 END)
  		   + (CASE WHEN pp.""HasAnimalSpecies"" THEN 10 ELSE 0 END)
  		WHEN pp.""Type"" = 2 THEN
  			(CASE WHEN pp.""HasText"" THEN 80 ELSE 0 END)
		   + (CASE WHEN pp.""HasEventDate"" THEN 20 ELSE 0 END)
		WHEN pp.""Type"" = 3 THEN
  			(CASE WHEN pp.""HasText"" THEN 70 ELSE 0 END)
		   + (CASE WHEN pp.""HasPhoto"" THEN 20 ELSE 0 END)
  		   + (CASE WHEN pp.""HasLocationAddress"" THEN 20 ELSE 0 END)
  		ELSE
		  CASE WHEN pp.""HasText"" THEN 100 ELSE 0 END 
	END AS ""CompletenessScore""
FROM (
	SELECT
		p.*,
		character_length(""Description"") > 0 AS ""HasText"",
		""MainPhotoId"" IS NOT NULL AS ""HasPhoto"",
		(SELECT COUNT(*) FROM PUBLIC.""Relations"" AS r WHERE r.""SourceId"" = p.""Id"" OR r.""DestinationId"" = p.""Id"") > 0 AS ""HasRelations"",
		json_extract_path_text(""Facts""::json, 'Main.Name', 'Values', '0', 'LastName') IS NOT NULL AS ""HasHumanName"",
		json_extract_path_text(""Facts""::json, 'Bio.Gender', 'IsMale') IS NOT NULL AS ""HasGender"",
		json_extract_path_text(""Facts""::json, 'Birth.Date', 'Value') IS NOT NULL AS ""HasBirthday"",
		json_extract_path_text(""Facts""::json, 'Birth.Place', 'Value') IS NOT NULL AS ""HasBirthPlace"",
		json_extract_path_text(""Facts""::json, 'Main.Name', 'Value') IS NOT NULL AS ""HasAnimalName"",
		json_extract_path_text(""Facts""::json, 'Bio.Species', 'Value') IS NOT NULL AS ""HasAnimalSpecies"",
		json_extract_path_text(""Facts""::json, 'Main.Location', 'Value') IS NOT NULL AS ""HasLocationAddress"",
		json_extract_path_text(""Facts""::json, 'Main.Date', 'Value') IS NOT NULL AS ""HasEventDate""
	FROM PUBLIC.""Pages"" AS ""p""
) AS pp
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW ""PagesScored""");
        }
    }
}
