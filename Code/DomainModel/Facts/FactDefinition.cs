using System;
using Bonsai.Code.DomainModel.Facts.Templates;

namespace Bonsai.Code.DomainModel.Facts
{
    /// <summary>
    /// Blueprint of a fact's template and editor.
    /// </summary>
    public class FactDefinition<TTemplate> : IFactDefinition
        where TTemplate: IFactTemplate
    {
        public FactDefinition(string id, string title)
        {
            Id = id;
            Title = title;
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
        /// Returns the path for display template.
        /// </summary>
        public string ViewTemplatePath => throw new NotImplementedException();

        /// <summary>
        /// Returns the path for editor template.
        /// </summary>
        public string EditTemplatePath => throw new NotImplementedException();
    }
}
