using System.Collections.Generic;
using Bonsai.Code.DomainModel.Facts.Templates;
using Bonsai.Data.Models;

namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// The mapping between facts and their corresponding templates.
    /// </summary>
    public static class FactDefinitions
    {
        public static Dictionary<PageType, FactDefinitionGroup[]> FactGroups = new Dictionary<PageType, FactDefinitionGroup[]>
        {
            [PageType.Person] = new[]
            {
                new FactDefinitionGroup(
                    "Common",
                    "Общее",
                    new FactDefinition<PhotoFactTemplate>("Photo", "Фото"),
                    new FactDefinition<NameFactTemplate>("Name", "Имя")
                ),
                new FactDefinitionGroup(
                    "Birth",
                    "Рождение",
                    new FactDefinition<DateFactTemplate>("Date", "Дата рождения"),
                    new FactDefinition<StringFactTemplate>("Place", "Место рождения")
                ),
                new FactDefinitionGroup(
                    "Death",
                    "Смерть",
                    new FactDefinition<DateFactTemplate>("Date", "Дата смерти"),
                    new FactDefinition<StringFactTemplate>("Place", "Место смерти"),
                    new FactDefinition<StringFactTemplate>("Cause", "Причина смерти"),
                    new FactDefinition<StringFactTemplate>("Burial", "Место захоронения")
                ),
                new FactDefinitionGroup(
                    "Bio",
                    "Биология",
                    new FactDefinition<GenderFactTemplate>("Gender", "Пол"),
                    new FactDefinition<BloodTypeFactTemplate>("Blood", "Группа крови"),
                    new FactDefinition<StringFactTemplate>("Eyes", "Цвет глаз"),
                    new FactDefinition<StringFactTemplate>("Hair", "Цвет волос")
                ),
                new FactDefinitionGroup(
                    "Person",
                    "Личность",
                    new FactDefinition<LanguageFactTemplate>("Language", "Язык"),
                    new FactDefinition<SkillFactTemplate>("Skill", "Умение"),
                    new FactDefinition<StringFactTemplate>("Religion", "Религия")
                )
            },

            [PageType.Pet] = new[]
            {
                new FactDefinitionGroup(
                    "Common",
                    "Общее",
                    new FactDefinition<PhotoFactTemplate>("Photo", "Фото"),
                    new FactDefinition<StringFactTemplate>("Name", "Имя")
                ),
                new FactDefinitionGroup(
                    "Birth",
                    "Рождение",
                    new FactDefinition<DateFactTemplate>("Date", "Дата рождения"),
                    new FactDefinition<StringFactTemplate>("Place", "Место рождения")
                ),
                new FactDefinitionGroup(
                    "Death",
                    "Смерть",
                    new FactDefinition<DateFactTemplate>("Date", "Дата смерти"),
                    new FactDefinition<StringFactTemplate>("Place", "Место смерти"),
                    new FactDefinition<StringFactTemplate>("Cause", "Причина смерти"),
                    new FactDefinition<StringFactTemplate>("Burial", "Место захоронения")
                ),
                new FactDefinitionGroup(
                    "Bio",
                    "Биология",
                    new FactDefinition<GenderFactTemplate>("Gender", "Пол"),
                    new FactDefinition<StringFactTemplate>("Species", "Вид"),
                    new FactDefinition<StringFactTemplate>("Breed", "Порода"),
                    new FactDefinition<StringFactTemplate>("Color", "Окрас")
                )
            },

            [PageType.Location] = new[]
            {
                new FactDefinitionGroup(
                    "Common",
                    "Общее",
                    new FactDefinition<PhotoFactTemplate>("Photo", "Фото"),
                    new FactDefinition<StringFactTemplate>("Name", "Название")
                ),
                new FactDefinitionGroup(
                    "Place",
                    "Место",
                    new FactDefinition<StringFactTemplate>("Location", "Расположение"),
                    new FactDefinition<DateFactTemplate>("Opening", "Открытие"),
                    new FactDefinition<DateFactTemplate>("Shutdown", "Закрытие")
                )
            },

            [PageType.Event] = new[]
            {
                new FactDefinitionGroup(
                    "Common",
                    "Общее",
                    new FactDefinition<PhotoFactTemplate>("Photo", "Фото"),
                    new FactDefinition<StringFactTemplate>("Name", "Название")
                ),
                new FactDefinitionGroup(
                    "Event",
                    "Событие",
                    new FactDefinition<DateFactTemplate>("Date", "Дата")
                )
            },

            [PageType.Other] = new FactDefinitionGroup[0]
        };
    }
}
