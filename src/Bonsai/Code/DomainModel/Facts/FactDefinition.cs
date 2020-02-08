using System;
using System.Linq;
using Bonsai.Code.DomainModel.Facts.Models;

namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// Blueprint of a fact's template and editor.
    /// </summary>
    public class FactDefinition<T> : IFactDefinition
        where T: FactModelBase
    {
        public FactDefinition(string id, string title, string shortTitle = null)
        {
            Id = id;
            Title = title;
            Kind = typeof(T);

            var parts = (shortTitle ?? title).Split('|');
            ShortTitle = shortTitle;
            ShortTitleSingle = parts.First();
            ShortTitleMultiple = parts.Last();
        }

        /// <summary>
        /// Unique ID for referencing the fact.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Readable title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The original short title.
        /// Required for json deserialization.
        /// </summary>
        public string ShortTitle { get; }

        /// <summary>
        /// Short title for displaying in the info block (with single value).
        /// </summary>
        public string ShortTitleSingle { get; }

        /// <summary>
        /// Short title for displaying in the info block (with multiple values).
        /// </summary>
        public string ShortTitleMultiple { get; }

        /// <summary>
        /// Type of the fact's kind.
        /// </summary>
        public Type Kind { get; }
    }

    /// <summary>
    /// Shared interface for untyped fact definitions.
    /// </summary>
    public interface IFactDefinition
    {
        /// <summary>
        /// Unique ID for referencing the fact.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Readable title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Short title for displaying in the info block.
        /// </summary>
        string ShortTitleSingle { get; }

        /// <summary>
        /// Short title for displaying in the info block (with multiple values).
        /// </summary>
        string ShortTitleMultiple { get; }

        /// <summary>
        /// Type of the fact's kind.
        /// </summary>
        Type Kind { get; }
    }
}
