using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Areas.Front.Logic
{
    /// <summary>
    /// The mapping between facts and their corresponding templates.
    /// </summary>
    public static class FactDefinitions
    {
        /// <summary>
        /// Fact-template mapping for a person.
        /// </summary>
        public static IReadOnlyDictionary<string, FactDefinition> PersonFacts = ToLookup(
            new FactDefinition("common.photo", "Фото", FactTemplate.Photo),
            new FactDefinition("common.name", "Имя", FactTemplate.Name),
            new FactDefinition("birth.date", "Дата рождения", FactTemplate.Date),
            new FactDefinition("birth.place", "Место рождения", FactTemplate.String),
            new FactDefinition("death.date", "Дата смерти", FactTemplate.Date),
            new FactDefinition("death.place", "Место смерти", FactTemplate.String),
            new FactDefinition("death.cause", "Причина смерти", FactTemplate.String),
            new FactDefinition("death.burial", "Место захоронения", FactTemplate.String),
            new FactDefinition("bio.gender", "Пол", FactTemplate.Gender),
            new FactDefinition("bio.blood", "Группа крови", FactTemplate.BloodType),
            new FactDefinition("bio.eyes", "Цвет глаз", FactTemplate.String),
            new FactDefinition("bio.hair", "Цвет волос", FactTemplate.String),
            new FactDefinition("person.language", "Язык", FactTemplate.Language),
            new FactDefinition("person.skill", "Умение", FactTemplate.Skill),
            new FactDefinition("person.religion", "Религия", FactTemplate.String)
        );

        /// <summary>
        /// Fact-template bindings for a pet.
        /// </summary>
        public static IReadOnlyDictionary<string, FactDefinition> PetFacts = ToLookup(
            new FactDefinition("common.photo", "Фото", FactTemplate.Photo),
            new FactDefinition("common.name", "Имя", FactTemplate.Name),
            new FactDefinition("birth.date", "Дата рождения", FactTemplate.Date),
            new FactDefinition("birth.place", "Место рождения", FactTemplate.String),
            new FactDefinition("death.date", "Дата смерти", FactTemplate.Date),
            new FactDefinition("death.place", "Место смерти", FactTemplate.String),
            new FactDefinition("death.cause", "Причина смерти", FactTemplate.String),
            new FactDefinition("death.burial", "Место захоронения", FactTemplate.String),
            new FactDefinition("bio.gender", "Пол", FactTemplate.Gender),
            new FactDefinition("bio.species", "Вид", FactTemplate.BloodType),
            new FactDefinition("bio.breed", "Порода", FactTemplate.String),
            new FactDefinition("bio.color", "Окрас", FactTemplate.String)
        );

        /// <summary>
        /// Fact-template bindings for a location.
        /// </summary>
        public static IReadOnlyDictionary<string, FactDefinition> PlaceFacts = ToLookup(
            new FactDefinition("common.photo", "Фото", FactTemplate.Photo),
            new FactDefinition("common.name", "Название", FactTemplate.Name),
            new FactDefinition("place.location", "Расположение", FactTemplate.String),
            new FactDefinition("place.opening", "Открытие", FactTemplate.Date),
            new FactDefinition("place.shutdown", "Закрытие", FactTemplate.Date)
        );

        /// <summary>
        /// Fact-template bindings for an event.
        /// </summary>
        public static IReadOnlyDictionary<string, FactDefinition> EventFacts = ToLookup(
            new FactDefinition("common.photo", "Фото", FactTemplate.Photo),
            new FactDefinition("common.name", "Название", FactTemplate.Name),
            new FactDefinition("event.date", "Дата", FactTemplate.Date)
        );

        /// <summary>
        /// Converts the list of fact definitions to a lookup table.
        /// </summary>
        private static IReadOnlyDictionary<string, FactDefinition> ToLookup(params FactDefinition[] defs)
        {
            return defs.ToDictionary(x => x.Id, x => x);
        }
    }
}
