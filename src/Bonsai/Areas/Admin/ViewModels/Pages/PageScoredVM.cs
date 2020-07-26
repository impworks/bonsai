using System;
using System.Collections.Generic;
using AutoMapper;
using Bonsai.Code.Infrastructure;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.ViewModels.Pages
{
    /// <summary>
    /// Page details with extended completeness scoring info.
    /// </summary>
    public class PageScoredVM: IMapped
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Page title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Page key (title, url-encoded).
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Type of the entity described by this page.
        /// </summary>
        public PageType Type { get; set; }

        /// <summary>
        /// Photograph for info block.
        /// </summary>
        public string MainPhotoPath { get; set; }

        /// <summary>
        /// Date of the page's creation.
        /// </summary>
        public DateTimeOffset CreationDate { get; set; }

        /// <summary>
        /// Date of the page's last revision.
        /// </summary>
        public DateTimeOffset LastUpdateDate { get; set; }

        public bool HasText { get; set; }
        public bool HasPhoto { get; set; }
        public bool HasRelations { get; set; }
        public bool HasGender { get; set; }
        public bool HasHumanName { get; set; }
        public bool HasAnimalName { get; set; }
        public bool HasAnimalSpecies { get; set; }
        public bool HasBirthday { get; set; }
        public bool HasBirthPlace { get; set; }
        public bool HasEventDate { get; set; }
        public bool HasLocationAddress { get; set; }

        /// <summary>
        /// Page completeness score (1..100) depending on page type and its content flags.
        /// </summary>
        public int CompletenessScore { get; set; }

        public IEnumerable<PageScoreCriterionVM> Criterions
        {
            get
            {
                yield return GetCriterion("Текст", x => x.HasText);

                if (Type == PageType.Person)
                {
                    yield return GetCriterion("Имя", x => x.HasHumanName);
                    yield return GetCriterion("День рождения", x => x.HasBirthday);
                    yield return GetCriterion("Место рождения", x => x.HasBirthPlace);
                    yield return GetCriterion("Пол", x => x.HasGender);
                    yield return GetCriterion("Фото", x => x.HasPhoto);
                    yield return GetCriterion("Связи", x => x.HasRelations);
                }

                if (Type == PageType.Pet)
                {
                    yield return GetCriterion("Имя", x => x.HasAnimalName);
                    yield return GetCriterion("День рождения", x => x.HasBirthday);
                    yield return GetCriterion("Вид", x => x.HasAnimalSpecies);
                    yield return GetCriterion("Фото", x => x.HasPhoto);
                    yield return GetCriterion("Связи", x => x.HasRelations);
                }

                if (Type == PageType.Event)
                {
                    yield return GetCriterion("Дата", x => x.HasEventDate);
                }

                if (Type == PageType.Location)
                {
                    yield return GetCriterion("Адрес", x => x.HasLocationAddress);
                    yield return GetCriterion("Фото", x => x.HasPhoto);
                }
            }
        }

        public void Configure(IProfileExpression profile)
        {
            profile.CreateMap<PageScored, PageScoredVM>()
                   .MapMember(x => x.Id, x => x.Id)
                   .MapMember(x => x.Title, x => x.Title)
                   .MapMember(x => x.Key, x => x.Key)
                   .MapMember(x => x.Type, x => x.Type)
                   .MapMember(x => x.MainPhotoPath, x => x.MainPhoto.FilePath)
                   .MapMember(x => x.CreationDate, x => x.CreationDate)
                   .MapMember(x => x.LastUpdateDate, x => x.LastUpdateDate)
                   .MapMember(x => x.HasText, x => x.HasText)
                   .MapMember(x => x.HasPhoto, x => x.HasPhoto)
                   .MapMember(x => x.HasRelations, x => x.HasRelations)
                   .MapMember(x => x.HasHumanName, x => x.HasHumanName)
                   .MapMember(x => x.HasAnimalName, x => x.HasAnimalName)
                   .MapMember(x => x.HasAnimalSpecies, x => x.HasAnimalSpecies)
                   .MapMember(x => x.HasBirthday, x => x.HasBirthday)
                   .MapMember(x => x.HasBirthPlace, x => x.HasBirthPlace)
                   .MapMember(x => x.HasEventDate, x => x.HasEventDate)
                   .MapMember(x => x.HasLocationAddress, x => x.HasLocationAddress)
                   .MapMember(x => x.CompletenessScore, x => x.CompletenessScore);
        }

        /// <summary>
        /// Creates the criterion for specified field.
        /// </summary>
        private PageScoreCriterionVM GetCriterion(string title, Func<PageScoredVM, bool> func)
        {
            return new PageScoreCriterionVM
            {
                Title = title,
                IsFilled = func(this)
            };
        }
    }
}
