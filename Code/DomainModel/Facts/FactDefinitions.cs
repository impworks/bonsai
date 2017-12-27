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
                    "common",
                    "Общее",
                    new FactDefinition<PhotoFactTemplate>("photo", "Фото"),
                    new FactDefinition<NameFactTemplate>("name", "Имя")
                ),
                new FactDefinitionGroup(
                    "birth",
                    "Рождение",
                    new FactDefinition<DateFactTemplate>("date", "Дата рождения"),
                    new FactDefinition<StringFactTemplate>("place", "Место рождения")
                ),
                new FactDefinitionGroup(
                    "death",
                    "Смерть",
                    new FactDefinition<DateFactTemplate>("date", "Дата смерти"),
                    new FactDefinition<StringFactTemplate>("place", "Место смерти"),
                    new FactDefinition<StringFactTemplate>("cause", "Причина смерти"),
                    new FactDefinition<StringFactTemplate>("burial", "Место захоронения")
                ),
                new FactDefinitionGroup(
                    "bio",
                    "Биология",
                    new FactDefinition<GenderFactTemplate>("gender", "Пол"),
                    new FactDefinition<BloodTypeFactTemplate>("blood", "Группа крови"),
                    new FactDefinition<StringFactTemplate>("eyes", "Цвет глаз"),
                    new FactDefinition<StringFactTemplate>("hair", "Цвет волос")
                ),
                new FactDefinitionGroup(
                    "person",
                    "Личность",
                    new FactDefinition<LanguageFactTemplate>("language", "Язык"),
                    new FactDefinition<SkillFactTemplate>("skill", "Умение"),
                    new FactDefinition<StringFactTemplate>("religion", "Религия")
                )
            },

            [PageType.Pet] = new[]
            {
                new FactDefinitionGroup(
                    "common",
                    "Общее",
                    new FactDefinition<PhotoFactTemplate>("photo", "Фото"),
                    new FactDefinition<StringFactTemplate>("name", "Имя")
                ),
                new FactDefinitionGroup(
                    "birth",
                    "Рождение",
                    new FactDefinition<DateFactTemplate>("date", "Дата рождения"),
                    new FactDefinition<StringFactTemplate>("place", "Место рождения")
                ),
                new FactDefinitionGroup(
                    "death",
                    "Смерть",
                    new FactDefinition<DateFactTemplate>("date", "Дата смерти"),
                    new FactDefinition<StringFactTemplate>("place", "Место смерти"),
                    new FactDefinition<StringFactTemplate>("cause", "Причина смерти"),
                    new FactDefinition<StringFactTemplate>("burial", "Место захоронения")
                ),
                new FactDefinitionGroup(
                    "bio",
                    "Биология",
                    new FactDefinition<GenderFactTemplate>("gender", "Пол"),
                    new FactDefinition<StringFactTemplate>("species", "Вид"),
                    new FactDefinition<StringFactTemplate>("breed", "Порода"),
                    new FactDefinition<StringFactTemplate>("color", "Окрас")
                )
            },

            [PageType.Location] = new[]
            {
                new FactDefinitionGroup(
                    "common",
                    "Общее",
                    new FactDefinition<PhotoFactTemplate>("photo", "Фото"),
                    new FactDefinition<StringFactTemplate>("name", "Название")
                ),
                new FactDefinitionGroup(
                    "place",
                    "Место",
                    new FactDefinition<StringFactTemplate>("location", "Расположение"),
                    new FactDefinition<DateFactTemplate>("opening", "Открытие"),
                    new FactDefinition<DateFactTemplate>("shutdown", "Закрытие")
                )
            },

            [PageType.Event] = new[]
            {
                new FactDefinitionGroup(
                    "common",
                    "Общее",
                    new FactDefinition<PhotoFactTemplate>("photo", "Фото"),
                    new FactDefinition<StringFactTemplate>("name", "Название")
                ),
                new FactDefinitionGroup(
                    "event",
                    "Событие",
                    new FactDefinition<DateFactTemplate>("date", "Дата")
                )
            },

            [PageType.Other] = new FactDefinitionGroup[0]
        };
    }
}
