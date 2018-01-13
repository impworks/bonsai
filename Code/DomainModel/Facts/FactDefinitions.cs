using System.Collections.Generic;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// The mapping between facts and their corresponding templates.
    /// </summary>
    public static class FactDefinitions
    {
        public static Dictionary<PageType, FactDefinitionGroup[]> Groups = new Dictionary<PageType, FactDefinitionGroup[]>
        {
            [PageType.Person] = new[]
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
                    new FactDefinition<DateFactModel>("Date", "Дата рождения", "Дата"),
                    new FactDefinition<StringFactModel>("Place", "Место рождения", "Место")
                ),
                new FactDefinitionGroup(
                    "Death",
                    "Смерть",
                    true,
                    new FactDefinition<DateFactModel>("Date", "Дата смерти", "Дата"),
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
                    new FactDefinition<StringFactModel>("Eyes", "Цвет глаз"),
                    new FactDefinition<StringFactModel>("Hair", "Цвет волос")
                ),
                new FactDefinitionGroup(
                    "Person",
                    "Личность",
                    false,
                    new FactDefinition<LanguageFactModel>("Language", "Язык"),
                    new FactDefinition<SkillFactModel>("Skill", "Хобби"),
                    new FactDefinition<StringListFactModel>("Profession", "Профессия"),
                    new FactDefinition<StringListFactModel>("Religion", "Религия")
                )
            },

            [PageType.Pet] = new[]
            {
                new FactDefinitionGroup(
                    "Main",
                    "Основное",
                    true,
                    new FactDefinition<StringFactModel>("Name", "Имя")
                ),
                new FactDefinitionGroup(
                    "Birth",
                    "Рождение",
                    true,
                    new FactDefinition<DateFactModel>("Date", "Дата рождения", "Дата"),
                    new FactDefinition<StringFactModel>("Place", "Место рождения", "Место")
                ),
                new FactDefinitionGroup(
                    "Death",
                    "Смерть",
                    true,
                    new FactDefinition<DateFactModel>("Date", "Дата смерти", "Дата"),
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
                    new FactDefinition<StringFactModel>("Name", "Название"),
                    new FactDefinition<StringFactModel>("Location", "Расположение"),
                    new FactDefinition<AnnotatedDateFactModel>("Opening", "Открытие"),
                    new FactDefinition<AnnotatedDateFactModel>("Shutdown", "Закрытие")
                )
            },

            [PageType.Event] = new[]
            {
                new FactDefinitionGroup(
                    "Main",
                    "Основное",
                    true,
                    new FactDefinition<StringFactModel>("Name", "Название"),
                    new FactDefinition<DateFactModel>("Date", "Дата")
                )
            },

            [PageType.Other] = new FactDefinitionGroup[0]
        };
    }
}
