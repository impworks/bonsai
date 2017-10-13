using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Code.Facts
{
    /// <summary>
    /// The reference list of all known facts.
    /// </summary>
    public static class KnownFacts
    {
        private const string TPL_DATE = "Date";
        private const string TPL_STRING = "String";
        private const string TPL_GENDER = "Gender";
        private const string TPL_BLOODTYPE = "BloodType";
        private const string TPL_PHOTO = "Photo";
        private const string TPL_LANGUAGE = "Language";

        static KnownFacts()
        {
            PersonFacts = new[]
            {
                new FactGroupDefinition(
                    "common",
                    "Общее",
                    new FactDefinition("photo", "Заглавное фото", TPL_PHOTO)
                ), 
                new FactGroupDefinition(
                    "birth",
                    "Рождение",
                    new FactDefinition("date", "Дата", TPL_DATE),
                    new FactDefinition("place", "Место", TPL_STRING)
                ),
                new FactGroupDefinition(
                    "death",
                    "Смерть",
                    new FactDefinition("date", "Дата", TPL_DATE),
                    new FactDefinition("place_death", "Место смерти", TPL_STRING),
                    new FactDefinition("cause", "Причина", TPL_STRING),
                    new FactDefinition("place_burial", "Место захоронения", TPL_STRING)
                ),
                new FactGroupDefinition(
                    "bio",
                    "Биология",
                    new FactDefinition("gender", "Пол", TPL_GENDER),
                    new FactDefinition("blood", "Группа крови", TPL_BLOODTYPE),
                    new FactDefinition("eyes", "Глаза", TPL_STRING),
                    new FactDefinition("hair", "Волосы", TPL_STRING)
                ),
                new FactGroupDefinition(
                    "name",
                    "Имя",
                    new FactDefinition("surname", "Фамилия", TPL_STRING),
                    new FactDefinition("name", "Имя", TPL_STRING),
                    new FactDefinition("patronym", "Отчество", TPL_STRING)
                ),
                new FactGroupDefinition(
                    "personality",
                    "Стороны личности",
                    new FactDefinition("language", "Язык", TPL_LANGUAGE),
                    new FactDefinition("skill", "Умение", TPL_STRING),
                    new FactDefinition("religion", "Религия", TPL_STRING)
                )
            }.ToDictionary(x => x.Key, x => x);
        }

        public static readonly IReadOnlyDictionary<string, FactGroupDefinition> PersonFacts;
    }
}
