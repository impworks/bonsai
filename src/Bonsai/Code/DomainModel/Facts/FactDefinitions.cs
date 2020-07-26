using System.Collections.Generic;
using System.Linq;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// The mapping between facts and their corresponding templates.
    /// </summary>
    public static class FactDefinitions
    {
        static FactDefinitions()
        {
            Groups = new Dictionary<PageType, FactDefinitionGroup[]>
            {
                [PageType.Person] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        "Основное",
                        true,
                        new FactDefinition<HumanNameFactModel>("Name", "Имя", "Имя|Имена")
                    ),
                    new FactDefinitionGroup(
                        "Birth",
                        "Рождение",
                        true,
                        new FactDefinition<BirthDateFactModel>("Date", "Дата рождения", "Дата"),
                        new FactDefinition<StringFactModel>("Place", "Место рождения", "Место")
                    ),
                    new FactDefinitionGroup(
                        "Death",
                        "Смерть",
                        true,
                        new FactDefinition<DeathDateFactModel>("Date", "Дата смерти", "Дата"),
                        new FactDefinition<StringFactModel>("Place", "Место смерти", "Место"),
                        new FactDefinition<StringFactModel>("Cause", "Причина смерти", "Причина"),
                        new FactDefinition<StringFactModel>("Burial", "Место захоронения")
                    ),
                    new FactDefinitionGroup(
                        "Bio",
                        "Биология",
                        false,
                        new FactDefinition<GenderFactModel>("Gender", "Пол"),
                        new FactDefinition<BloodTypeFactModel>("Blood", "Группа крови", "Гр. крови"),
                        new FactDefinition<StringFactModel>("Eyes", "Цвет глаз", "Глаза"),
                        new FactDefinition<StringFactModel>("Hair", "Цвет волос", "Волосы")
                    ),
                    new FactDefinitionGroup(
                        "Person",
                        "Личность",
                        false,
                        new FactDefinition<LanguageFactModel>("Language", "Язык", "Язык|Языки"),
                        new FactDefinition<SkillFactModel>("Skill", "Хобби"),
                        new FactDefinition<StringListFactModel>("Profession", "Профессия", "Профессия|Профессии"),
                        new FactDefinition<StringListFactModel>("Religion", "Религия", "Религия|Религии")
                    )
                },

                [PageType.Pet] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        "Основное",
                        true,
                        new FactDefinition<NameFactModel>("Name", "Имя")
                    ),
                    new FactDefinitionGroup(
                        "Birth",
                        "Рождение",
                        true,
                        new FactDefinition<BirthDateFactModel>("Date", "Дата рождения", "Дата"),
                        new FactDefinition<StringFactModel>("Place", "Место рождения", "Место")
                    ),
                    new FactDefinitionGroup(
                        "Death",
                        "Смерть",
                        true,
                        new FactDefinition<DeathDateFactModel>("Date", "Дата смерти", "Дата"),
                        new FactDefinition<StringFactModel>("Place", "Место смерти", "Место"),
                        new FactDefinition<StringFactModel>("Cause", "Причина смерти", "Причина"),
                        new FactDefinition<StringFactModel>("Burial", "Место захоронения")
                    ),
                    new FactDefinitionGroup(
                        "Bio",
                        "Биология",
                        true,
                        new FactDefinition<GenderFactModel>("Gender", "Пол"),
                        new FactDefinition<StringFactModel>("Species", "Вид"),
                        new FactDefinition<StringFactModel>("Breed", "Порода"),
                        new FactDefinition<StringFactModel>("Color", "Окрас")
                    )
                },

                [PageType.Location] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        "Основное",
                        true,
                        new FactDefinition<AddressFactModel>("Location", "Адрес"),
                        new FactDefinition<DateFactModel>("Opening", "Приобретение"),
                        new FactDefinition<DateFactModel>("Shutdown", "Продажа")
                    )
                },

                [PageType.Event] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        "Основное",
                        true,
                        new FactDefinition<DateFactModel>("Date", "Дата")
                    )
                },

                [PageType.Other] = new FactDefinitionGroup[0]
            };

            Definitions = Groups.ToDictionary(
                x => x.Key,
                x => x.Value.SelectMany(y => y.Defs.Select(z => new { Key = y.Id + "." + z.Id, Fact = z }))
                      .ToDictionary(y => y.Key, y => y.Fact)
            );
        }

        /// <summary>
        /// Available groups of fact definitions.
        /// </summary>
        public static readonly Dictionary<PageType, FactDefinitionGroup[]> Groups;

        /// <summary>
        /// Lookup for fact definitions.
        /// </summary>
        public static readonly Dictionary<PageType, Dictionary<string, IFactDefinition>> Definitions;

        /// <summary>
        /// Finds a definition.
        /// </summary>
        public static IFactDefinition TryGetDefinition(PageType type, string key)
        {
            return Definitions.TryGetValue(type, out var pageLookup)
                   && pageLookup.TryGetValue(key, out var def)
                ? def
                : null;
        }
    }
}
