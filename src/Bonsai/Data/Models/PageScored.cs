using System;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Augmented page info with completeness score calculated in the SQL view.
    /// </summary>
    public class PageScored
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
        public Media MainPhoto { get; set; }

        /// <summary>
        /// FK of the main photo.
        /// </summary>
        public Guid? MainPhotoId { get; set; }

        /// <summary>
        /// Date of the page's creation.
        /// </summary>
        public DateTimeOffset CreationDate { get; set; }

        /// <summary>
        /// Date of the page's last revision.
        /// </summary>
        public DateTimeOffset LastUpdateDate { get; set; }

        /// <summary>
        /// Flag indicating that this resource is removed.
        /// </summary>
        public bool IsDeleted { get; set; }

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
    }
}
