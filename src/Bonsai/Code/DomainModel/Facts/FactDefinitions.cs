using System.Collections.Generic;
using System.Linq;
using Bonsai.Code.DomainModel.Facts.Models;
using Bonsai.Data.Models;
using Bonsai.Localization;

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
                        Texts.Facts_Group_Main,
                        true,
                        new FactDefinition<HumanNameFactModel>("Name", Texts.Facts_Person_Name, Texts.Facts_Person_NameS)
                    ),
                    new FactDefinitionGroup(
                        "Birth",
                        Texts.Facts_Group_Birth,
                        true,
                        new FactDefinition<BirthDateFactModel>("Date", Texts.Facts_Birth_Day, Texts.Facts_Birth_DayS),
                        new FactDefinition<StringFactModel>("Place", Texts.Facts_Birth_Place, Texts.Facts_Birth_PlaceS)
                    ),
                    new FactDefinitionGroup(
                        "Death",
                        Texts.Facts_Group_Death,
                        true,
                        new FactDefinition<DeathDateFactModel>("Date", Texts.Facts_Death_Date, Texts.Facts_Death_DateS),
                        new FactDefinition<StringFactModel>("Place", Texts.Facts_Death_Place, Texts.Facts_Death_PlaceS),
                        new FactDefinition<StringFactModel>("Cause", Texts.Facts_Death_Cause, Texts.Facts_Death_CauseS),
                        new FactDefinition<StringFactModel>("Burial", Texts.Facts_Death_Burial, Texts.Facts_Death_BurialS)
                    ),
                    new FactDefinitionGroup(
                        "Bio",
                        Texts.Facts_Group_Bio,
                        false,
                        new FactDefinition<GenderFactModel>("Gender", Texts.Facts_Person_Gender),
                        new FactDefinition<BloodTypeFactModel>("Blood", Texts.Facts_Person_BloodType, Texts.Facts_Person_BloodTypeS),
                        new FactDefinition<StringFactModel>("Eyes", Texts.Facts_Person_EyeColor, Texts.Facts_Person_EyeColorS),
                        new FactDefinition<StringFactModel>("Hair", Texts.Facts_Person_HairColor, Texts.Facts_Person_HairColorS)
                    ),
                    new FactDefinitionGroup(
                        "Person",
                        Texts.Facts_Group_Person,
                        false,
                        new FactDefinition<LanguageFactModel>("Language", Texts.Facts_Person_Language, Texts.Facts_Person_LanguageS),
                        new FactDefinition<SkillFactModel>("Skill", Texts.Facts_Person_Skill),
                        new FactDefinition<StringListFactModel>("Profession", Texts.Facts_Person_Profession, Texts.Facts_Person_ProfessionS),
                        new FactDefinition<StringListFactModel>("Religion", Texts.Facts_Person_Religion, Texts.Facts_Person_ReligionS)
                    ),
                    new FactDefinitionGroup(
                        "Meta",
                        Texts.Facts_Group_Meta,
                        false,
                        new FactDefinition<ContactsFactModel>("SocialProfiles", Texts.Facts_Person_SocialProfiles)
                    )
                },

                [PageType.Pet] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        Texts.Facts_Group_Main,
                        true,
                        new FactDefinition<NameFactModel>("Name", Texts.Facts_Pet_Name)
                    ),
                    new FactDefinitionGroup(
                        "Birth",
                        Texts.Facts_Group_Birth,
                        true,
                        new FactDefinition<BirthDateFactModel>("Date", Texts.Facts_Birth_Day, Texts.Facts_Birth_DayS),
                        new FactDefinition<StringFactModel>("Place", Texts.Facts_Birth_Place, Texts.Facts_Birth_PlaceS)
                    ),
                    new FactDefinitionGroup(
                        "Death",
                        Texts.Facts_Group_Death,
                        true,
                        new FactDefinition<DeathDateFactModel>("Date", Texts.Facts_Death_Date, Texts.Facts_Death_DateS),
                        new FactDefinition<StringFactModel>("Place", Texts.Facts_Death_Place, Texts.Facts_Death_PlaceS),
                        new FactDefinition<StringFactModel>("Cause", Texts.Facts_Death_Cause, Texts.Facts_Death_CauseS),
                        new FactDefinition<StringFactModel>("Burial", Texts.Facts_Death_Burial, Texts.Facts_Death_BurialS)
                    ),
                    new FactDefinitionGroup(
                        "Bio",
                        Texts.Facts_Group_Bio,
                        true,
                        new FactDefinition<GenderFactModel>("Gender", Texts.Facts_Pet_Gender),
                        new FactDefinition<StringFactModel>("Species", Texts.Facts_Pet_Species),
                        new FactDefinition<StringFactModel>("Breed",  Texts.Facts_Pet_Breed),
                        new FactDefinition<StringFactModel>("Color",  Texts.Facts_Pet_Color)
                    )
                },

                [PageType.Location] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        Texts.Facts_Group_Main,
                        true,
                        new FactDefinition<AddressFactModel>("Location", Texts.Facts_Location_Address),
                        new FactDefinition<DateFactModel>("Opening", Texts.Facts_Location_Opening),
                        new FactDefinition<DateFactModel>("Shutdown", Texts.Facts_Location_Shutdown)
                    )
                },

                [PageType.Event] = new[]
                {
                    new FactDefinitionGroup(
                        "Main",
                        Texts.Facts_Group_Main,
                        true,
                        new FactDefinition<DateFactModel>("Date", Texts.Facts_Event_Date)
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
