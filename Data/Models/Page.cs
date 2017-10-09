using System;

namespace Bonsai.Data.Models
{
    /// <summary>
    /// Page describing a single entity.
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Surrogate ID.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// Page title.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// Page URL.
        /// </summary>
        public virtual string Url { get; set; }

        /// <summary>
        /// Type of the entity described by this page.
        /// </summary>
        public virtual PageType PageType { get; set; }

        /// <summary>
        /// Free text description of the entity.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Serialized facts collection.
        /// </summary>
        public virtual string Facts { get; set; }
    }
}
