using System;
using System.Collections.Generic;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;
using Bonsai.Localization;
using Mapster;

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
                yield return GetCriterion(Texts.Admin_Pages_Criterion_Text, x => x.HasText);

                if (Type == PageType.Person)
                {
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_HumanName, x => x.HasHumanName);
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Birthday, x => x.HasBirthday);
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Birthplace, x => x.HasBirthPlace);
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Gender, x => x.HasGender);
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Photo, x => x.HasPhoto);
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Relations, x => x.HasRelations);
                }

                if (Type == PageType.Pet)
                {
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_AnimalName, x => x.HasAnimalName);
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Birthday, x => x.HasBirthday);
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Species, x => x.HasAnimalSpecies);
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Photo, x => x.HasPhoto);
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Relations, x => x.HasRelations);
                }

                if (Type == PageType.Event)
                {
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Date, x => x.HasEventDate);
                }

                if (Type == PageType.Location)
                {
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Address, x => x.HasLocationAddress);
                    yield return GetCriterion(Texts.Admin_Pages_Criterion_Photo, x => x.HasPhoto);
                }
            }
        }

        public void Configure(TypeAdapterConfig config)
        {
            config.NewConfig<PageScored, PageScoredVM>()
                  .Map(x => x.Id, x => x.Id)
                  .Map(x => x.Title, x => x.Title)
                  .Map(x => x.Key, x => x.Key)
                  .Map(x => x.Type, x => x.Type)
                  .Map(x => x.MainPhotoPath, x => x.MainPhoto.FilePath)
                  .Map(x => x.CreationDate, x => x.CreationDate)
                  .Map(x => x.LastUpdateDate, x => x.LastUpdateDate)
                  .Map(x => x.HasText, x => x.HasText)
                  .Map(x => x.HasPhoto, x => x.HasPhoto)
                  .Map(x => x.HasRelations, x => x.HasRelations)
                  .Map(x => x.HasHumanName, x => x.HasHumanName)
                  .Map(x => x.HasAnimalName, x => x.HasAnimalName)
                  .Map(x => x.HasAnimalSpecies, x => x.HasAnimalSpecies)
                  .Map(x => x.HasBirthday, x => x.HasBirthday)
                  .Map(x => x.HasBirthPlace, x => x.HasBirthPlace)
                  .Map(x => x.HasEventDate, x => x.HasEventDate)
                  .Map(x => x.HasLocationAddress, x => x.HasLocationAddress)
                  .Map(x => x.CompletenessScore, x => x.CompletenessScore);
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
