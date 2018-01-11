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
                    "Common",
                    "Общее",
                    new FactDefinition<NameFactModel>("Name", "Имя")
                ),
                new FactDefinitionGroup(
                    "Birth",
                    "Рождение",
                    new FactDefinition<DateFactModel>("Date", "Дата рождения"),
                    new FactDefinition<StringFactModel>("Place", "Место рождения")
                ),
                new FactDefinitionGroup(
                    "Death",
                    "Смерть",
                    new FactDefinition<DateFactModel>("Date", "Дата смерти"),
                    new FactDefinition<StringFactModel>("Place", "Место смерти"),
                    new FactDefinition<StringFactModel>("Cause", "Причина смерти"),
                    new FactDefinition<StringFactModel>("Burial", "Место захоронения")
                ),
                new FactDefinitionGroup(
                    "Bio",
                    "Биология",
                    new FactDefinition<GenderFactModel>("Gender", "Пол"),
                    new FactDefinition<BloodTypeFactModel>("Blood", "Группа крови"),
                    new FactDefinition<StringFactModel>("Eyes", "Цвет глаз"),
                    new FactDefinition<StringFactModel>("Hair", "Цвет волос")
                ),
                new FactDefinitionGroup(
                    "Person",
                    "Личность",
                    new FactDefinition<LanguageFactModel>("Language", "Язык"),
                    new FactDefinition<SkillFactModel>("Skill", "Хобби"),
                    new FactDefinition<StringFactModel>("Religion", "Религия")
                )
            },

            [PageType.Pet] = new[]
            {
                new FactDefinitionGroup(
                    "Common",
                    "Общее",
                    new FactDefinition<StringFactModel>("Name", "Имя")
                ),
                new FactDefinitionGroup(
                    "Birth",
                    "Рождение",
                    new FactDefinition<DateFactModel>("Date", "Дата рождения"),
                    new FactDefinition<StringFactModel>("Place", "Место рождения")
                ),
                new FactDefinitionGroup(
                    "Death",
                    "Смерть",
                    new FactDefinition<DateFactModel>("Date", "Дата смерти"),
                    new FactDefinition<StringFactModel>("Place", "Место смерти"),
                    new FactDefinition<StringFactModel>("Cause", "Причина смерти"),
                    new FactDefinition<StringFactModel>("Burial", "Место захоронения")
                ),
                new FactDefinitionGroup(
                    "Bio",
                    "Биология",
                    new FactDefinition<GenderFactModel>("Gender", "Пол"),
                    new FactDefinition<StringFactModel>("Species", "Вид"),
                    new FactDefinition<StringFactModel>("Breed", "Порода"),
                    new FactDefinition<StringFactModel>("Color", "Окрас")
                )
            },

            [PageType.Location] = new[]
            {
                new FactDefinitionGroup(
                    "Common",
                    "Общее",
                    new FactDefinition<StringFactModel>("Name", "Название")
                ),
                new FactDefinitionGroup(
                    "Place",
                    "Место",
                    new FactDefinition<StringFactModel>("Location", "Расположение"),
                    new FactDefinition<AnnotatedDateFactModel>("Opening", "Открытие"),
                    new FactDefinition<AnnotatedDateFactModel>("Shutdown", "Закрытие")
                )
            },

            [PageType.Event] = new[]
            {
                new FactDefinitionGroup(
                    "Common",
                    "Общее",
                    new FactDefinition<StringFactModel>("Name", "Название")
                ),
                new FactDefinitionGroup(
                    "Event",
                    "Событие",
                    new FactDefinition<DateFactModel>("Date", "Дата")
                )
            },

            [PageType.Other] = new FactDefinitionGroup[0]
        };
    }
}
