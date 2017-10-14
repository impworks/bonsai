using System.Collections.Generic;

namespace Bonsai.Areas.Front.Logic
{
    public static class FactTemplatesMap
    {
        private const string TPL_DATE = "Date";
        private const string TPL_STRING = "String";
        private const string TPL_GENDER = "Gender";
        private const string TPL_BLOODTYPE = "BloodType";
        private const string TPL_PHOTO = "Photo";
        private const string TPL_NAME = "Name";
        private const string TPL_LANGUAGE = "Language";
        private const string TPL_SKILL = "Skill";

        /// <summary>
        /// Fact-template mapping for a person.
        /// </summary>
        public static IReadOnlyDictionary<string, string> PersonFacts = new Dictionary<string, string>
        {
            ["common.photo"] = TPL_PHOTO,
            ["common.name"] = TPL_NAME,
            ["birth.date"] = TPL_DATE,
            ["birth.place"] = TPL_STRING,
            ["death.date"] = TPL_DATE,
            ["death.place"] = TPL_STRING,
            ["death.cause"] = TPL_STRING,
            ["death.burial"] = TPL_STRING,
            ["bio.gender"] = TPL_GENDER,
            ["bio.blood"] = TPL_BLOODTYPE,
            ["bio.eyes"] = TPL_STRING,
            ["bio.hair"] = TPL_STRING,
            ["person.language"] = TPL_LANGUAGE,
            ["person.skill"] = TPL_SKILL,
            ["person.religion"] = TPL_STRING,
        };
    }
}
