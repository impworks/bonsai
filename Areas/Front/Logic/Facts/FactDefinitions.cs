using System.Collections.Generic;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Front.Logic.Facts
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
                    new FactDefinition("photo", "Фото", FactTemplate.Photo),
                    new FactDefinition("name", "Имя", FactTemplate.Name)
                ),
                new FactDefinitionGroup(
                    "birth",
                    "Рождение",
                    new FactDefinition("date", "Дата рождения", FactTemplate.Date),
                    new FactDefinition("place", "Место рождения", FactTemplate.String)
                ),
                new FactDefinitionGroup(
                    "death",
                    "Смерть",
                    new FactDefinition("date", "Дата смерти", FactTemplate.Date),
                    new FactDefinition("place", "Место смерти", FactTemplate.String),
                    new FactDefinition("cause", "Причина смерти", FactTemplate.String),
                    new FactDefinition("burial", "Место захоронения", FactTemplate.String)
                ),
                new FactDefinitionGroup(
                    "bio",
                    "Биология",
                    new FactDefinition("gender", "Пол", FactTemplate.Gender),
                    new FactDefinition("blood", "Группа крови", FactTemplate.BloodType),
                    new FactDefinition("eyes", "Цвет глаз", FactTemplate.String),
                    new FactDefinition("hair", "Цвет волос", FactTemplate.String)
                ),
                new FactDefinitionGroup(
                    "person",
                    "Личность",
                    new FactDefinition("language", "Язык", FactTemplate.Language),
                    new FactDefinition("skill", "Умение", FactTemplate.Skill),
                    new FactDefinition("religion", "Религия", FactTemplate.String)
                )
            },

            [PageType.Pet] = new[]
            {
                new FactDefinitionGroup(
                    "common",
                    "Общее",
                    new FactDefinition("photo", "Фото", FactTemplate.Photo),
                    new FactDefinition("name", "Имя", FactTemplate.Name)
                ),
                new FactDefinitionGroup(
                    "birth",
                    "Рождение",
                    new FactDefinition("date", "Дата рождения", FactTemplate.Date),
                    new FactDefinition("place", "Место рождения", FactTemplate.String)
                ),
                new FactDefinitionGroup(
                    "death",
                    "Смерть",
                    new FactDefinition("date", "Дата смерти", FactTemplate.Date),
                    new FactDefinition("place", "Место смерти", FactTemplate.String),
                    new FactDefinition("cause", "Причина смерти", FactTemplate.String),
                    new FactDefinition("burial", "Место захоронения", FactTemplate.String)
                ),
                new FactDefinitionGroup(
                    "bio",
                    "Биология",
                    new FactDefinition("gender", "Пол", FactTemplate.Gender),
                    new FactDefinition("species", "Вид", FactTemplate.String),
                    new FactDefinition("breed", "Порода", FactTemplate.String),
                    new FactDefinition("color", "Окрас", FactTemplate.String)
                )
            },

            [PageType.Location] = new[]
            {
                new FactDefinitionGroup(
                    "common",
                    "Общее",
                    new FactDefinition("photo", "Фото", FactTemplate.Photo),
                    new FactDefinition("name", "Название", FactTemplate.String)
                ),
                new FactDefinitionGroup(
                    "place",
                    "Место",
                    new FactDefinition("location", "Расположение", FactTemplate.String),
                    new FactDefinition("opening", "Открытие", FactTemplate.Date),
                    new FactDefinition("shutdown", "Закрытие", FactTemplate.Date)
                )
            },

            [PageType.Event] = new[]
            {
                new FactDefinitionGroup(
                    "common",
                    "Общее",
                    new FactDefinition("photo", "Фото", FactTemplate.Photo),
                    new FactDefinition("name", "Название", FactTemplate.String)
                ),
                new FactDefinitionGroup(
                    "event",
                    "Событие",
                    new FactDefinition("date", "Дата", FactTemplate.Date)
                )
            },

            [PageType.Other] = new FactDefinitionGroup[0]
        };
    }
}
